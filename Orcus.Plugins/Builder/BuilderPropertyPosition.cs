using Orcus.Shared.Core;

namespace Orcus.Plugins.Builder
{
    /// <summary>
    ///     The position of the <see cref="IBuilderPropertyView" /> in the builder
    /// </summary>
    public class BuilderPropertyPosition
    {
        protected BuilderPropertyPosition(BuilderCategory builderCategory)
        {
            BuilderCategory = builderCategory;
        }

        /// <summary>
        ///     The category of the <see cref="IBuilderPropertyView" />
        /// </summary>
        public BuilderCategory BuilderCategory { get; set; }

        /// <summary>
        ///     The group of the <see cref="IBuilderPropertyView" />
        /// </summary>
        public BuilderGroup BuilderGroup { get; set; }

        /// <summary>
        ///     The index of the <see cref="IBuilderPropertyView" />
        /// </summary>
        public BuilderPropertyIndex BuilderPropertyIndex { get; set; }

        /// <summary>
        ///     Defines whether the builder property is the leader of the <see cref="BuilderGroup" />
        /// </summary>
        public bool IsGroupLeader { get; set; }

        /// <summary>
        ///     Initialize a new <see cref="BuilderPropertyPosition" />
        /// </summary>
        /// <param name="builderCategory">The category of the <see cref="IBuilderPropertyView" /></param>
        /// <returns>Return the <see cref="BuilderPropertyPosition" /> which represents the give category</returns>
        public static BuilderPropertyPosition FromCategory(BuilderCategory builderCategory)
        {
            return new BuilderPropertyPosition(builderCategory);
        }

        /// <summary>
        ///     Set the group of the builder property
        /// </summary>
        /// <param name="builderGroup">The builder group</param>
        public BuilderPropertyPosition InGroup(BuilderGroup builderGroup)
        {
            BuilderGroup = builderGroup;
            return this;
        }

        /// <summary>
        ///     Set the <see cref="IBuilderPropertyView" /> as the leader of the group
        /// </summary>
        public BuilderPropertyPosition SetLeader()
        {
            IsGroupLeader = true;
            return this;
        }

        /// <summary>
        ///     Set the index of the builder property
        /// </summary>
        /// <typeparam name="T">The type of the builder property which comes before</typeparam>
        public BuilderPropertyPosition ComesAfter<T>() where T : IBuilderProperty
        {
            BuilderPropertyIndex = BuilderPropertyIndex.AfterBuilderProperty<T>();
            return this;
        }
    }
}