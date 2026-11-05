# CheckoutHandlers.AuthorizeNetApi

Authorize.Net checkout handler for DynamicWeb 10, built on **Accept Hosted** (the Authorize.Net hosted payment page), **customer profiles** for saved cards and **webhooks** for capture, refund and void synchronization.

Targets .NET 8 and DynamicWeb 10.26 or later, like the other Dynamicweb checkout handlers, so it installs on .NET 8 and .NET 10 hosts.

## Features

| Capability | How |
| --- | --- |
| Card payment | Customer is redirected to the Authorize.Net hosted payment page. Card data never touches the DynamicWeb server. |
| Authorization only or authorization + capture | `The type of credit card transaction` parameter. |
| Capture from DynamicWeb (`IRemoteCapture`) | `priorAuthCaptureTransaction` for the full order amount. Authorize.Net allows one capture per authorization. |
| Cancel (`ICancelOrder`) | `voidTransaction`. Only possible until the daily batch settles. |
| Full and partial refund (`IFullReturn`, `IPartialReturn`) | `refundTransaction`. Only possible after the transaction has settled; unsettled transactions must be voided instead. |
| Saved cards (`ISavedCardAction`) | Stored as Authorize.Net customer payment profiles (`createCustomerProfileFromTransactionRequest`). The DynamicWeb user id is the `merchantCustomerId`. |
| Webhooks (`ICheckoutHandlerCallback`) | Signature-verified notifications update capture, refund and void state and complete orders paid on the hosted page even when the customer never returns. |

## Setup

### 1. Authorize.Net account

In the Merchant Interface (**Account > Settings > Security Settings**) you need:

- **API Login ID** and **Transaction Key** (API Credentials & Keys)
- **Signature Key** (also under API Credentials & Keys). It is required: webhooks are rejected without it.
- **Transaction Details API** enabled (**Account > Settings > Security Settings > Transaction Details API**). It is required: the handler reads transactions back (`getTransactionDetailsRequest`, `getUnsettledTransactionListRequest`) to verify payments and to process webhooks. Without it those calls fail with error `E00011`, every payment stays "Pending verification" and no webhook is processed. The log message names this setting.

Use a [sandbox account](https://developer.authorize.net/hello_world/sandbox.html) with **Test mode** enabled on the payment method while testing.

### 2. Payment method in DynamicWeb

Create a payment method with the checkout handler **Authorize.Net API** and fill in:

| Parameter | Description |
| --- | --- |
| API login ID, Transaction key, Signature key | From the Merchant Interface. |
| Allow save cards | Lets logged-in customers store their card for later purchases. |
| The type of credit card transaction | *Authorization and Capture* (money is taken immediately) or *Authorization only* (capture later from the order list). |
| Test mode | Uses the Authorize.Net sandbox endpoints. |
| Cancel template, Error template, Pending template | Templates under `Templates/eCom7/CheckoutHandler/AuthorizeNet/Cancel`, `.../Error` and `.../Pending`. Default templates are installed with the package. The error template receives `CheckoutHandler:ErrorMessage`, the cancel template `CheckoutHandler:CancelMessage` and the pending template `CheckoutHandler:PendingMessage` and `CheckoutHandler:CheckAgainUrl` (a link that verifies the payment again). |
| Enable debug logging | Logs every request and response (secrets and card numbers masked) to the event log and the order's debugging info. |
| Enable auto webhook registration | Registers the webhook subscription in Authorize.Net on the first checkout (checked at most once per hour). |
| Force webhook re-registration | Deletes and recreates the subscription on the next checkout, then resets itself. |

### 3. Webhooks

Webhooks are **required**. Payments on the hosted page are verified when the customer returns, but the card type, masked card number and the capture, refund and void states come from webhooks, and an order whose customer closes the browser after paying is only completed by the webhook.

The callback URL is:

```
https://<your site>/dwapi/ecommerce/carts/callback/Authorize.Net%20API
```

Either enable **auto webhook registration** or create the webhook yourself in the Merchant Interface (**Account > Settings > Business Settings > Webhooks**) with these events:

- `net.authorize.payment.authorization.created`
- `net.authorize.payment.authcapture.created`
- `net.authorize.payment.capture.created`
- `net.authorize.payment.priorAuthCapture.created`
- `net.authorize.payment.refund.created`
- `net.authorize.payment.void.created`
- `net.authorize.payment.fraud.held`, `net.authorize.payment.fraud.approved`, `net.authorize.payment.fraud.declined`

The site must be reachable from the internet over HTTPS for webhooks to arrive.

## How a payment flows

1. `BeginCheckout` requests a hosted form token (`getHostedPaymentPageRequest`) with the order amount, line items, addresses and the return URLs, and auto-submits the customer to the hosted page.
2. The customer pays on the Authorize.Net page and is redirected back with `action=Receipt` (or `action=Cancel`).
3. The redirect carries no transaction data, so `HandleRequest` looks up the newest unsettled transaction for the order (`getUnsettledTransactionListRequest`, the 1000 most recent), reads it (`getTransactionDetailsRequest`), checks the response code and that the amount matches the order, and only then completes the order. When no transaction can be found yet, the order is **not** downgraded (the customer may have paid): it is marked "Pending verification", the pending template is shown and the webhook completes the order. If the customer checks out the same cart again, the handler first looks for the earlier payment and completes the order with it instead of charging again.
4. Authorize.Net sends a webhook for the transaction. The signature (`X-ANET-Signature`, HMAC-SHA512 with the signature key) is verified and the transaction is re-read from the API. Webhooks arrive for every transaction on the merchant account, including ones created outside DynamicWeb (Virtual Terminal, other systems, other sites on the same account), so the transaction must belong to the order before anything is changed: its `transId` or `refTransId` must be the order's transaction number, or, before the order has one, its invoice number must be the order id. Then the order is updated: transaction number, card data, capture state, saved card. If the customer never returned, the webhook completes the order, after the same amount check as the receipt path. A transaction held for review by the Authorize.Net fraud filters completes the order with the status "Held for Review" on both paths, also after the amount check. Approve or decline it in the Merchant Interface: the fraud.approved webhook records the capture (authorization and capture) or leaves it pending capture (authorization only). fraud.declined sets the status "Declined after review" and logs an error; the order stays complete, so **do not ship orders in that status**.
5. Capture, cancel and refunds from the order list call the API directly; the matching webhooks are recognised as already processed (refund transactions are tracked by a `[RefundTransId:...]` marker in the return operation).

The customer's return and the webhooks of one order are serialized by an in-process lock and re-check whether the order is already complete, so the order is completed (stock, mails, `CheckoutDone`) only once. On a host with several instances behind a load balancer the two requests can still overlap.

Every request carries `DW-{AutoId}` as `refId`; Authorize.Net echoes it as `merchantReferenceId` in webhooks, which is how a notification is matched to its order even after the cart id has changed to an order id. `invoiceNumber` (an order id) is the fallback.

Webhooks are always answered with HTTP 200, also when processing fails (the error is logged on the order). Authorize.Net retries other answers up to ten times and then deactivates the webhook for the whole account.

Saved-card payments are flagged as stored-credential transactions (`processingOptions.isStoredCredentials`), as the [card-on-file rules](https://developer.authorize.net/api/reference/features/card-on-file.html) require; the hosted-form payment that stores the card is not.

## Amounts and currencies

Amounts are sent with up to four decimals in the currency of the order. Authorize.Net processes transactions in the currency of the merchant account; make sure the DynamicWeb currency matches it.

## Development

```
dotnet build Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.sln
dotnet test tests/Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Tests
```

The unit tests cover signature validation, log masking, enum and payload parsing, request/response shapes, the webhook registration policy, order matching, and API round trips through a stub HTTP handler (request bodies, error envelopes, the `E00011` hint).

CI follows the other Dynamicweb checkout handlers: `pr.yml` and `main.yml` call the common build and NuGet push workflows in `dynamicweb/Common.Actions`, and `build-and-test.yml` runs the unit tests, which the common build does not. The test project lives in its own folder under `tests/` because the common build evaluates `**/*.csproj` from the repository root. End-to-end testing needs a DynamicWeb 10 site with a public URL and an Authorize.Net sandbox account: run a checkout with the [test card numbers](https://developer.authorize.net/hello_world/testing_guide.html), then capture, cancel and refund the order from the order list and check the order's debugging info.

Project layout:

- `src/AuthorizeNetCheckoutHandler.cs` – the add-in: checkout, callbacks, capture/cancel/return, saved cards
- `src/Services/AuthorizeNetService.cs` – typed API operations; `AuthorizeNetHttpService.cs` – HTTP, error envelopes, logging
- `src/Helpers` – signature validation, masking, order/webhook helpers
- `src/Models` – Authorize.Net request and response contracts
- `src/Updates` – update provider that installs the default cancel, error and pending templates
- `tests/Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Tests` – xUnit tests
