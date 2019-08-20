using System;
using System.Collections.Generic;

namespace Assets.Scripts
{
    public class InterfacePendingAction
    {
        public Dictionary<UIPendingActionParam, object> Parameters = new Dictionary<UIPendingActionParam, object>();
        public Action<Dictionary<UIPendingActionParam, object>> PendingAction;

        public void AddOrReplaceParameter(UIPendingActionParam type, object value)
        {
            if (Parameters.TryGetValue(type, out object _))
                Parameters[UIPendingActionParam.CurrentCell] = value;
            else
                Parameters.Add(UIPendingActionParam.CurrentCell, value);
        }
    }
}
