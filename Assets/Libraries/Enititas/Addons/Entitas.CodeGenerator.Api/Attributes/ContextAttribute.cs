using System;

namespace Entitas.CodeGenerator.Api {

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
    public class ContextAttribute : Attribute {

        public readonly string contextName;

        public ContextAttribute(string contextName) {
            this.contextName = contextName;
        }
    }
}
