using System;
using System.Collections.Generic;
using System.IdentityModel.Selectors;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hadouken.Http.Api
{
    public class IdentityValidator : UserNamePasswordValidator
    {
        public override void Validate(string userName, string password)
        {
        }
    }
}
