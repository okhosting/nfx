/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2017 ITAdapter Corp. Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NFX.Wave.MVC
{
  /// <summary>
  /// Decorates controller classes or actions that need to check CSRF token on POST against the user session
  /// </summary>
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
  public sealed class SessionCSRFCheckAttribute : ActionFilterAttribute
  {
    public const string DEFAULT_TOKEN_NAME = CoreConsts.CSRF_TOKEN_NAME;


    public SessionCSRFCheckAttribute() : base(0)
    {
      TokenName = DEFAULT_TOKEN_NAME;
      OnlyExistingSession = true;
    }

    public SessionCSRFCheckAttribute(string tokenName) : this(tokenName, true, 0)
    {
    }

    public SessionCSRFCheckAttribute(string tokenName, bool onlyExistingSession, int order) : base(order)
    {
      TokenName = tokenName;

      if (TokenName.IsNullOrWhiteSpace())
        TokenName = DEFAULT_TOKEN_NAME;

      OnlyExistingSession = onlyExistingSession;
    }


    public string TokenName{ get; private set; }
    public bool OnlyExistingSession{ get; private set; }


    protected internal override bool BeforeActionInvocation(Controller controller, WorkContext work, string action, MethodInfo method, object[] args, ref object result)
    {
      if (work.IsGET) return false;

      work.NeedsSession(OnlyExistingSession);

      var session = work.Session;
      var supplied = work.WholeRequestAsJSONDataMap[TokenName].AsString();

      var bad = session==null;

      if (!bad && session.LastLoginType!=ApplicationModel.SessionLoginType.Robot)
         bad = !session.CSRFToken.EqualsOrdSenseCase(supplied);

      if (bad) throw new HTTPStatusException(NFX.Web.WebConsts.STATUS_400, NFX.Web.WebConsts.STATUS_400_DESCRIPTION, "CSRF failed");

      return false;
    }

    protected internal override bool AfterActionInvocation(Controller controller, WorkContext work, string action, MethodInfo method, object[] args, ref object result)
    {
      return false;
    }

    protected internal override void ActionInvocationFinally(Controller controller, WorkContext work, string action, MethodInfo method, object[] args, ref object result)
    {

    }
  }
}
