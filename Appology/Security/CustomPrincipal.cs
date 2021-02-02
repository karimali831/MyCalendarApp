using Appology.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace Appology.Security
{
    public class CustomPrincipal : IPrincipal
    {
        private User Account;

        public CustomPrincipal(User account)
        {
            Account = account;
            Identity = new GenericIdentity(account.Email);
        }

        public IIdentity Identity { get; set; }

        public bool IsInRole(string role)
        {
            var roles = role.Split(new char[] { ',' });
            return roles.Any(r => Account.RoleIds.Contains(r));
        }
    }
}