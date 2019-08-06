using System;
using System.Collections.Generic;

namespace Assets.Scripts
{
    public class InterfacePendingAction
    {
        public Dictionary<InterfacePendingActionParamType, object> Parameters = new Dictionary<InterfacePendingActionParamType, object>();
        public Action<Dictionary<InterfacePendingActionParamType, object>> PendingAction;

        public void AddOrReplaceParameter(InterfacePendingActionParamType type, object value)
        {
            if (Parameters.TryGetValue(type, out object _))
                Parameters[InterfacePendingActionParamType.CurrentCell] = value;
            else
                Parameters.Add(InterfacePendingActionParamType.CurrentCell, value);
        }
    }
}
