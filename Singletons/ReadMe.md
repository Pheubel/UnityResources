# Source
Author: Benjamin Moir \
Repository: https://github.com/NewBloodInteractive/com.newblood.core \
Thread: https://twitter.com/TheZombieKiller/status/1488926918712107009 \
License: https://github.com/NewBloodInteractive/com.newblood.core/blob/master/LICENSE.md

# Details
A singleton implementation that aims to solve common headaches found with other implementations.

# Citations
Let's talk singletons. Love or hate them, they're an extremely common pattern in Unity games—for good reason. The engine itself even has some built-in! (kinda)

However, I often see developers implement the pattern in very error-prone or overly-complex ways.

Consider the following example. It is representative of what you will typically see in tutorials and various other educational resources for Unity: \
![common singleton example](/Singletons/ReadMeResources/CommonSingleton.png)

Simple as it may be, this has a number of glaring issues:

* If a singleton needs to access another singleton, it cannot reliably do so in Awake.
* The component needs to be manually placed in every scene.
* You need to manually implement the pattern for every singleton.

\
So how are these problems typically solved?

The first two issues are commonly solved by implementing "get or create" functionality, often using `FindObjectOfType`.
This "works", but can be dreadfully slow and isn't enough to handle a non-trivial component. \
![FindObjectOfType example](/Singletons/ReadMeResources/FindObjectOfType.png)

I have seen developers use many other approaches (such as virtual Awake methods), but they all share the same core problems.

Additionally, implementing this manually on every singleton has just become a larger burden.

That much is at least solved with a generic base class: \
![generic base class example](/Singletons/ReadMeResources/GenericbaseClass.png)

Apart from some extra code to handle various edge cases, this is effectively the extent of most singleton implementations I have come across in Unity.

So why is this bad?

1. While we are no longer required to manually place singleton objects into scenes, this is still necessary if the singleton needs references to assets in the project (such as ScriptableObjects) \
\
The Instance property will attempt to create the component if it does not exist, meaning that these fields will not be assigned if the developer has forgotten to do this. We no longer have reliable initialization for our component.

2. Depending on which approach you have taken to implementing the pattern, you may have taken "ownership" of the Awake method. \
\
While you can make this method virtual, it's easy to forget to override it—because this is not necessary when writing a normal component. \
![awake ownership issue example](/Singletons/ReadMeResources/AwakeOwnershipIssue.png)

3. If we remove the functionality to create the component when it is not found, we will find ourselves with a large performance hit when forgetting to place one in a scene, the Instance property will always call `FindObjectOfType` (a rather slow method) and fail.

I could continue to list problems with all these approaches, but this thread is already getting quite long.

So how can we implement this better? We'll do so in two stages:

1. Write a system for persistent components.
2. Implement MonoSingleton on top of this.

The first stage is comprised of implementing two types: `IPersistentObject` and `PersistentObjects`. The former simply allows a component to mark itself as persistent and initialize itself: \
![IPersistentObject interface](/Singletons/ReadMeResources/IPersistantObject.png)

The `PersistentObjects` class is where the magic happens. It contains a single method: `Initialize`, which receives a prefab object acting as a container for all of our persistent objects: \
![PersistentObjects class](/Singletons/ReadMeResources/PersistentObjects.png) \
(in Unity view) \
![PersistentObjects in Unity](/Singletons/ReadMeResources/PersistentObjectsInUnity.png)

For such a small method, there is a lot going on in there. Allow me to explain how this works: \
We:
* Create a "root" GameObject
* Disable it
* Instantiate our prefab as a child

Why is this important?

The disabled parent object ensures that no callbacks will run yet, not even `Awake`!

This means we can initialize our objects before they do anything. \
![prevent awake callback snippet](/Singletons/ReadMeResources/SnippetPreventAwakeCallback.png)

This part is quite simple: we look for components implementing our `IPersistentObject` interface (including inactive objects is important) and call the `Initialize` method.

For our `MonoSingleton` class, this will assign the `Instance` property, making it available when `Awake` runs! \
![initialize persistent objects snippet](/Singletons/ReadMeResources/SnippetInitializePersistentObjects.png)

We now mark the root object as persistent, which will move it to a special `DontDestroyOnLoad` scene.

This allows us to simply unpack the hierarchy of our prefab (using `DetachChildren`) to "spill" our singletons into that scene.

Then we can destroy the root, it's no longer needed. \
![spill children snippet](/Singletons/ReadMeResources/SnippetSpillChildren.png)

When we unpack the hierarchy of our prefab instance, the objects are no longer children of a disabled GameObject. As a result, their callbacks will be allowed to start executing.

That's the persistent object implementation out of the way, but how do we actually wire it all up?

Unity exposes an attribute to enable you to run code when the game starts: `[RuntimeInitializeOnLoadMethod]`.

We will use this to call `PersistentObjects.Initialize`.

We retrieve our prefab object (you can do this however you wish), and then provide it to the `PersistentObjects.Initialize` method. \
![PersistentObjects initialization example](/Singletons/ReadMeResources/PersistentObjectsInitializationExample.png)

Now that we have a way to write persistent components, implementing MonoSingleton is quite trivial: \
![MonoSingleton class](/Singletons/ReadMeResources/MonoSingleton.png)

We use a private protected constructor to ensure that nobody else can inherit from `MonoSingleton` directly, they must go through the generic `MonoSingleton<T>` class. \
![MonoSingleton constructor](/Singletons/ReadMeResources/MonoSingletonConstructor.png)

We explicitly implement `IPersistentObject.Initialize`, as there is little reason to make it public when it is effectively an alias for `MakeCurrent`. \
![explicit implementation of IPersistentObject.Initialize](/Singletons/ReadMeResources/ExplicitImplementationInitialize.png)

Finally, the `MonoSingleton<T>` class overrides and seals `MakeCurrent` so that derived classes cannot override it themselves and break our expectations. \
![implementation of MakeCurrent](/Singletons/ReadMeResources/MakeCurrentImplementation.png)

With all of these pieces in place, our `GameController` component is extremely simple to implement, with none of the drawbacks of the other approaches: \
![example MonoSingleton component implementation](/Singletons/ReadMeResources/MonoSingletonComponentExample.png)

And there we have it: the full implementation, barely approaching 50 lines total! (`PersistentObjectsInitializer` is a per-project class, so it is not included here.) \
![full implementation of all parts](/Singletons/ReadMeResources/FullImplementation.png)