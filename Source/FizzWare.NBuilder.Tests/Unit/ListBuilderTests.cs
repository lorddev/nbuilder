﻿using System.Collections.Generic;
using FizzWare.NBuilder.Implementation;
using FizzWare.NBuilder.PropertyNaming;
using FizzWare.NBuilder.Tests.TestClasses;
using NUnit.Framework;
using Rhino.Mocks;

namespace FizzWare.NBuilder.Tests.Unit
{
    [TestFixture]
    public class ListBuilderTests
    {
        private IReflectionUtil reflectionUtil;
        private IPropertyNamer propertyNamer;
        private readonly MyClass myClass = new MyClass();
        private const int listSize = 10;

        private MockRepository mocks;

        [SetUp]
        public void SetUp()
        {
            mocks = new MockRepository();

            reflectionUtil = mocks.StrictMock<IReflectionUtil>();
            propertyNamer = mocks.StrictMock<IPropertyNamer>();
        }

        [TearDown]
        public void TearDown()
        {
            mocks.VerifyAll();
        }

        [Test]
        public void ShouldConstructDeclarations()
        {
            IGlobalDeclaration<MyClass> declaration = MockRepository.GenerateMock<IGlobalDeclaration<MyClass>>();

            var builder = new ListBuilder<MyClass>(listSize, propertyNamer, reflectionUtil, new BuilderSettings());

            builder.AddDeclaration(declaration);

            using (mocks.Record())
                declaration.Expect(x => x.Construct());

            using (mocks.Playback())
                builder.Construct();
        }

        [Test]
        public void ConstructShouldComplainIfTypeNotParameterlessNoAllAndSumOfItemsInDeclarationsDoNotEqualCapacity()
        {
            IDeclaration<MyClassWithConstructor> declaration1 = MockRepository.GenerateMock<IDeclaration<MyClassWithConstructor>>();
            IDeclaration<MyClassWithConstructor> declaration2 = MockRepository.GenerateMock<IDeclaration<MyClassWithConstructor>>();

            using (mocks.Record())
            {
                declaration1.Expect(x => x.NumberOfAffectedItems).Return(2);
                declaration2.Expect(x => x.NumberOfAffectedItems).Return(2);
                reflectionUtil.Expect(x => x.RequiresConstructorArgs(typeof (MyClass))).Return(true);
            }

            var builder = new ListBuilder<MyClass>(10, propertyNamer, reflectionUtil,new BuilderSettings());

            using (mocks.Playback())
            {
                // TODO FIX
                #if !SILVERLIGHT
                Assert.Throws<BuilderException>(() => builder.Construct());
                #endif
            }
        }

        [Test]
        public void Constructing_AssignsValuesToProperties()
        {
            using (mocks.Record())
                propertyNamer.Expect(x => x.SetValuesOfAllIn(Arg<IList<MyClass>>.Is.TypeOf));

            var list = new List<MyClass>();
            
            var builder = new ListBuilder<MyClass>(listSize, propertyNamer, reflectionUtil, new BuilderSettings());

            using (mocks.Playback())
                builder.Name(list);
        }

        [Test]
        public void ShouldBeAbleToBuildAList()
        {
            IDeclaration<MyClass> declaration = MockRepository.GenerateMock<IDeclaration<MyClass>>();
           
            var builder = new ListBuilder<MyClass>(listSize, propertyNamer, reflectionUtil, new BuilderSettings());

            using (mocks.Record())
            {
                reflectionUtil.Stub(x => x.RequiresConstructorArgs(typeof (MyClass))).Return(false).Repeat.Any();
                reflectionUtil.Expect(x => x.CreateInstanceOf<MyClass>()).Return(myClass).Repeat.Any();
                declaration.Expect(x => x.Construct());
                declaration.Expect(x => x.AddToMaster(Arg<MyClass[]>.Is.TypeOf));
                propertyNamer.Expect(x => x.SetValuesOfAllIn(Arg<IList<MyClass>>.Is.TypeOf));
                declaration.Expect(x => x.CallFunctions(Arg<IList<MyClass>>.Is.TypeOf)).IgnoreArguments();
            }

            using (mocks.Playback())
                builder.Build();
        }

        [Test]
        public void IfNoAllExistsAndSumOfAffectedItemsInDeclarationsIsLessThanCapacity_ShouldAddADefaultAll()
        {
            var builder = new ListBuilder<MyClass>(30, propertyNamer, reflectionUtil, new BuilderSettings());
            builder.TheFirst(10);

            using (mocks.Record())
            {
                reflectionUtil.Expect(x => x.RequiresConstructorArgs(typeof (MyClass))).Return(false).Repeat.Any();

                // Even though a declaration of 10 has been added, we expect the list builder to add
                // a default GlobalDeclaration (All). Therefore we expect CreateInstanceOf to be called 40 times
                reflectionUtil.Expect(x => x.CreateInstanceOf<MyClass>()).Return(myClass).Repeat.Times(40);
            }

            using (mocks.Playback())
                builder.Construct();
        }
    }
}