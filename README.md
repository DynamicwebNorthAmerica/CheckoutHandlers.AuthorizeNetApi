# CheckoutHandlers.AuthorizeNetApi

Authorize.Net checkout handler for DynamicWeb 10, built on **Accept Hosted** (the Authorize.Net hosted payment page), **customer profiles** for saved cards and **webhooks** for capture, refund and void synchronization.

Targets .NET 10 and DynamicWeb 10.30.

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

Use a [sandbox account](https://developer.authorize.net/hello_world/sandbox.html) with **Test mode** enabled on the payment method while testing.

### 2. Payment method in DynamicWeb

Create a payment method with the checkout handler **Authorize.Net API** and fill in:

| Parameter | Description |
| --- | --- |
| API login ID, Transaction key, Signature key | From the Merchant Interface. |
| Allow save cards | Lets logged-in customers store their card for later purchases. |
| The type of credit card transaction | *Authorization and Capture* (money is taken immediately) or *Authorization only* (capture later from the order list). |
| Test mode | Uses the Authorize.Net sandbox endpoints. |
| Cancel template, Error template | Templates under `Templates/eCom7/CheckoutHandler/AuthorizeNet/Cancel` and `.../Error`. The error template receives `CheckoutHandler:ErrorMessage`. |
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
3. The redirect carries no transaction data, so `HandleRequest` looks up the newest unsettled transaction for the order (`getUnsettledTransactionListRequest`), reads it (`getTransactionDetailsRequest`), checks the response code and that the amount matches the order, and only then completes the order. An order that cannot be verified is downgraded to a cart with an error.
4. Authorize.Net sends a webhook for the transaction. The signature (`X-ANET-Signature`, HMAC-SHA512 with the signature key) is verified, the transaction is re-read from the API and the order is updated: transaction number, card data, capture state, saved card. If the customer never returned, the webhook completes the order, after the same amount check as the receipt path. A transaction held for review by the Authorize.Net fraud filters completes the order with the status "Held for Review" on both paths. Approve or decline it in the Merchant Interface: the fraud.approved webhook re-reads the transaction and records the capture (authorization and capture) or leaves it pending capture (authorization only); fraud.declined marks the order declined.
5. Capture, cancel and refunds from the order list call the API directly; the matching webhooks are recognised as already processed (refund transactions are tracked by a `[RefundTransId:...]` marker in the return operation).

Every request carries the order's `AutoId` as `refId`; Authorize.Net echoes it as `merchantReferenceId` in webhooks, which is how a notification is matched to its order even after the cart id has changed to an order id. `invoiceNumber` is the fallback.

## Amounts and currencies

Amounts are sent with up to four decimals in the currency of the order. Authorize.Net processes transactions in the currency of the merchant account; make sure the DynamicWeb currency matches it.

## Development

```
dotnet build Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.sln
dotnet test tests/Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Tests.csproj
```

The unit tests cover signature validation, log masking, enum and payload parsing, request/response shapes and the webhook registration policy. End-to-end testing needs a DynamicWeb 10 site with a public URL and an Authorize.Net sandbox account: run a checkout with the [test card numbers](https://developer.authorize.net/hello_world/testing_guide.html), then capture, cancel and refund the order from the order list and check the order's debugging info.

Project layout:

- `src/AuthorizeNetCheckoutHandler.cs` – the add-in: checkout, callbacks, capture/cancel/return, saved cards
- `src/Services/AuthorizeNetService.cs` – typed API operations; `AuthorizeNetHttpService.cs` – HTTP, error envelopes, logging
- `src/Helpers` – signature validation, masking, order/webhook helpers
- `src/Models` – Authorize.Net request and response contracts
- `tests` – xUnit tests
