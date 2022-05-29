# How to Dependency Inject

The concept for this implementation of DI was inspired by ASP.NET Core-style DI (https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.1). So if you're already familiar with that, then this should feel similar.

## Summary

You use an `IServiceCollection` to setup your dependencies and an `IServiceManager` to request instances of services at runtime. USITools comes with concrete implementations of these interfaces called simply `ServiceCollection` and `ServiceManager` that should suffice for most use cases. The `ServiceCollection` will create a `ServiceDefinition` for each service you register that `ServiceManager` then uses to determine how to instantiate your services.

### The really cool thing about this system...

...is that `ServiceManager` will automagically instantiate all the dependencies required by the services you request. So if you have a class `Widget` that requires an instance of `Gizmo`, the `ServiceManager` will automatically create a `Gizmo` and pass it to the `Widget` via it's constructor before passing the `Widget` back to you. Neat! It's even smart enough to know that if `Gizmo` is a singleton, to pass the singleton instance of it to `Widget` instead of creating a new instance. This eliminates the need to do things like setup static instance properties on a class and use getters that instantiate singletons as side effects. Bleck.

## Registering Services

The initial setup is fairly simple:

* Create a `ServiceCollection` to register all the service classes that will participate in DI. Service classes can be registered several different ways:
  * Typically, you should be using interfaces in order to facilitate unit testing. That's kinda the whole reason we use DI in the first place. So constructors should be written to accept interfaces as parameters and service classes should be registered in DI using the `Add...` methods that accept **two** type parameters. The first parameter is the interface type, the second is the concrete type that implements the interface.
  * If you want to inject something like a Unity or KSP class as a dependency, the `Add...` methods that accept a single type parameter will happily allow that.
  * For services that should only ever be instantiated once and reused, use one of the `AddSingleton...` methods.
  * If you want a new instance of a service every time you request the service from `ServiceManager`, use the `AddService...` methods.

## Consuming Services

* Create a `ServiceManager`, passing in your `ServiceCollection` via its constructor, and then call one of the `Get...` methods. That's all there is to it!
* For services that are registered via interface, you will pass the **interface** type to the `Get...` method, not the concrete type. The `ServiceManager` will know which concrete implementation to return based on what you configured in the `ServiceCollection`.
* The `ServiceCollection` and `ServiceManager` have to be bootstrapped somehow. We currently do it in a `KSPAddon`.
* There can be multiple `ServiceManager` instances running at once. They don't interoperate though. So if you have a class with dependencies handled by different `ServiceManager` instances, you will only be able to get automatic dependency injection from one of them. That said, you could probaby accomplish some trickery using the `IDependencyService` interface explained in the MonoBehaviours section of this document.

## MonoBehaviours

`MonoBehaviour` classes require a different approach in order to be used with DI because they can't accept parameters via constructor. `IDependencyService` to the rescue! The `ServiceManager` will check each registered service type to see if it's assignable from `IDependencyService` and if so, it will bypass automatic dependency injection and call the service's `SetServiceManager` method instead, passing itself in as the paramater.

**Note: Classes that implement `IDependencyService` must have a parameterless constructor.**

The `MonoBehaviour` then will have to be responsible for requesting its dependencies from the `ServiceManager` manually. The upside is that `ServiceManager` will automatically create game objects for you, prevent them from being destroyed if they are registered as singletons and will keep track of singleton `MonoBehaviour` instances for you just like it would with a normal service.

`MonoBehaviour`s are registered in the `ServiceCollection` via the `AddMonoBehaviour...` methods.

## Dev Notes

* `IServiceCollection` is setup for fluent method chaining to make setting up multiple services a little easier.
* The order services are registered doesn't matter.
* Services that implement the same interface can't be registered in the `ServiceCollection` at the same time. That is to say, they can't both be registered implementations of the interface. One could be registered as a standalone service but then why have it implement the same interface as the other? If you find yourself wanting to register multiple classes in DI under the same interface, you probably need to reconsider your design.
* `MonoBehaviour`s should be used sparingly. They're best used as wrappers around a POCO that can participate fully in DI.