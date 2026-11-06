using Dynamicweb.Updates;
using System;
using System.Collections.Generic;
using System.IO;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Updates;

/// <summary>
/// Installs the default cancel, error and pending templates when the package is installed or updated.
/// Existing files are not overwritten.
/// </summary>
public sealed class AuthorizeNetUpdateProvider : UpdateProvider
{
    private static Stream GetResourceStream(string name)
    {
        string resourceName = $"Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Updates.{name}";
        Stream? stream = typeof(AuthorizeNetUpdateProvider).Assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
            throw new InvalidOperationException($"Embedded resource '{resourceName}' was not found.");

        return stream;
    }

    public override IEnumerable<Update> GetUpdates()
    {
        return
        [
            new FileUpdate("b1e75683-708a-4015-95f0-6bdd7422b0de", this, "/Files/Templates/eCom7/CheckoutHandler/AuthorizeNet/Cancel/checkouthandler_cancel.html", () => GetResourceStream("checkouthandler_cancel.html")),
            new FileUpdate("aa3a4e66-674b-4dc7-bd04-8a48a3e7e64d", this, "/Files/Templates/eCom7/CheckoutHandler/AuthorizeNet/Error/checkouthandler_error.html", () => GetResourceStream("checkouthandler_error.html")),
            new FileUpdate("0f5b790a-2c99-477b-9fe9-98f0c6e6d54c", this, "/Files/Templates/eCom7/CheckoutHandler/AuthorizeNet/Pending/checkouthandler_pending.html", () => GetResourceStream("checkouthandler_pending.html"))
        ];
    }

    /*
     * IMPORTANT!
     * Use a generated GUID string as id for an update
     * - Execute command in C# interactive window: Guid.NewGuid().ToString()
     */
}
