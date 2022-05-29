using UnityEngine;

namespace USITools.Tests.Unit
{
    interface ITestInterface { }

    class TestMonoBehaviourImplementingInterface : MonoBehaviour, ITestInterface
    {
        public TestMonoBehaviourImplementingInterface() { }
    }

    class TestServiceImplmentingInterface : ITestInterface
    {
        public TestServiceImplmentingInterface() { }
    }

    class AnotherTestMonoBehaviourImplementingInterface : MonoBehaviour, ITestInterface
    {
        public AnotherTestMonoBehaviourImplementingInterface() { }
    }

    class AnotherTestServiceImplementingInterface : ITestInterface
    {
        public AnotherTestServiceImplementingInterface() { }
    }

    class TestMonoBehaviourWithoutInterface : MonoBehaviour
    {
        public TestMonoBehaviourWithoutInterface() { }
    }

    class TestServiceWithoutInterface
    {
        public TestServiceWithoutInterface() { }
    }

    class TestServiceWithInterfaceDependency
    {
        public ITestInterface Dependency { get; private set; }
        public TestServiceWithInterfaceDependency(ITestInterface dependency)
        {
            Dependency = dependency;
        }
    }

    class TestServiceWithClassDependency
    {
        public TestServiceWithoutInterface Dependency { get; private set; }
        public TestServiceWithClassDependency(TestServiceWithoutInterface dependency)
        {
            Dependency = dependency;
        }
    }

    class TestServiceWithInterfaceAndClassDependencies
    {
        public ITestInterface InterfaceDependency { get; private set; }
        public TestServiceWithoutInterface ClassDependency { get; private set; }
        public TestServiceWithInterfaceAndClassDependencies(ITestInterface interfaceDependency, TestServiceWithoutInterface classDependency)
        {
            InterfaceDependency = interfaceDependency;
            ClassDependency = classDependency;
        }
    }

    class TestServiceWithMultipleConstructors
    {
        public TestServiceWithMultipleConstructors(ITestInterface dependency) { }
        public TestServiceWithMultipleConstructors(TestServiceWithoutInterface dependency) { }
    }
}
