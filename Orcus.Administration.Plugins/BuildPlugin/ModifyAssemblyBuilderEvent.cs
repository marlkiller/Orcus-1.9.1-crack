using Mono.Cecil;

namespace Orcus.Administration.Plugins.BuildPlugin
{
    /// <summary>
    ///     Make changes to the IL code of Orcus
    /// </summary>
    public class ModifyAssemblyBuilderEvent : BuilderEvent
    {
        /// <summary>
        ///     Make changes to the IL code of Orcus
        /// </summary>
        /// <param name="builderInformation">The builder settings provide the current properties like file names</param>
        /// <param name="assemblyDefinition">The assembly definition can be used to edit the IL code</param>
        public delegate void ModifyAssemblyDelegate(
            IBuilderInformation builderInformation, AssemblyDefinition assemblyDefinition);

        /// <summary>
        ///     Initialize a new instance of <see cref="ModifyAssemblyBuilderEvent" />
        /// </summary>
        /// <param name="modifyAssemblyDelegate">The delegate to execute</param>
        public ModifyAssemblyBuilderEvent(ModifyAssemblyDelegate modifyAssemblyDelegate)
        {
            ModifyAssembly = modifyAssemblyDelegate;
        }

        public ModifyAssemblyDelegate ModifyAssembly { get; }
    }
}