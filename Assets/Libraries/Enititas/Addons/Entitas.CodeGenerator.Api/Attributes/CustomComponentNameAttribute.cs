﻿using System;

namespace Entitas.CodeGenerator.Api {

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Struct)]
    public class CustomComponentNameAttribute : Attribute {

        public readonly string[] componentNames;

        public CustomComponentNameAttribute(params string[] componentNames) {
            this.componentNames = componentNames;
        }
    }
}
