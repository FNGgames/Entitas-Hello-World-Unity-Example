using System.IO;
using System.Linq;

namespace Entitas.CodeGenerator {

    public class MatcherGenerator : ICodeGenerator {

        public string name { get { return "Matcher"; } }
        public bool isEnabledByDefault { get { return true; } }

        const string MATCHER_TEMPLATE =
@"public sealed partial class ${ContextName}Matcher {

    static Entitas.IMatcher<${ContextName}Entity> _matcher${ComponentName};

    public static Entitas.IMatcher<${ContextName}Entity> ${ComponentName} {
        get {
            if(_matcher${ComponentName} == null) {
                var matcher = (Entitas.Matcher<${ContextName}Entity>)Entitas.Matcher<${ContextName}Entity>.AllOf(${Index});
                matcher.componentNames = ${ComponentNames};
                _matcher${ComponentName} = matcher;
            }

            return _matcher${ComponentName};
        }
    }
}
";

        public CodeGenFile[] Generate(CodeGeneratorData[] data) {
            return data
                .OfType<ComponentData>()
                .Where(d => d.ShouldGenerateIndex())
                .SelectMany(d => generateMatcher(d))
                .ToArray();
        }

        CodeGenFile[] generateMatcher(ComponentData data) {
            return data.GetContextNames()
                       .Select(context => generateMatcher(context, data))
                       .ToArray();
        }

        CodeGenFile generateMatcher(string contextName, ComponentData data) {
            var componentName = data.GetFullTypeName().ToComponentName();
            var index = contextName + ComponentsLookupGenerator.COMPONENTS_LOOKUP + "." + componentName;
            var componentNames = contextName + ComponentsLookupGenerator.COMPONENTS_LOOKUP + ".componentNames";

            var fileContent = MATCHER_TEMPLATE
                .Replace("${ContextName}", contextName)
                .Replace("${ComponentName}", componentName)
                .Replace("${Index}", index)
                .Replace("${ComponentNames}", componentNames);

            return new CodeGenFile(
                contextName + Path.DirectorySeparatorChar +
                "Components" + Path.DirectorySeparatorChar +
                contextName + componentName.AddComponentSuffix() + ".cs",
                fileContent,
                GetType().FullName
            );
        }
    }
}
