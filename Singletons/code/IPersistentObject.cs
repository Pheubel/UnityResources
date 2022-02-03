// source: https://github.com/NewBloodInteractive/com.newblood.core/blob/master/Runtime/IPersistentObject.cs
// alterations: the namespace has been changed to signal the focus on the signleton feature.

namespace Singleton
{
    /// <summary>Provides a callback for initializing components with <see cref="PersistentObjects"/>.</summary>
    public interface IPersistentObject
    {
        /// <summary>Performs component-specific initialization.</summary>
        void Initialize();
    }
}