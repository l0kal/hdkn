using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IdentityModel.Selectors;
using Hadouken.Common.Security;

namespace Hadouken.Common.Http.HttpListener
{
    internal class IdentityValidator : UserNamePasswordValidator
    {
        private readonly string _userName;
        private readonly string _password;

        public IdentityValidator(string userName, string password)
        {
            _userName = userName;
            _password = password;
        }

        public override void Validate(string userName, string password)
        {
            if (!(String.Equals(_userName, userName) && String.Equals(_password, Hash.Generate(password))))
                throw new UnauthorizedAccessException();
        }
    }
}
