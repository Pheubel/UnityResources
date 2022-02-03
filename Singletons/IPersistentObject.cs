// source: https://github.com/NewBloodInteractive/com.newblood.core/blob/master/Runtime/IPersistentObject.cs
// alterations: the namespace has been changed to be part of the UnityResources package.

namespace UnityResources.Singleton
{
    /// <summary>Provides a callback for initializing components with <see cref="PersistentObjects"/>.</summary>
    public interface IPersistentObject
    {
        /// <summary>Performs component-specific initialization.</summary>
        void Initialize();
    }
}