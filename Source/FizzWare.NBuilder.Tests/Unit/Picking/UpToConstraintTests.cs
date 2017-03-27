﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSubstitute;
using NUnit.Framework;

namespace FizzWare.NBuilder.Tests.Unit.Picking
{
    [TestFixture]
    public class UpToConstraintTests
    {
        private IUniqueRandomGenerator uniqueRandomGenerator;
        private const int count = 5;

        [SetUp]
        public void SetUp()
        {
            uniqueRandomGenerator = Substitute.For<IUniqueRandomGenerator>();
        }

        [Test]
        public void ShouldBeAbleToUseBetweenPickerConstraint()
        {
            var constraint = new UpToConstraint(uniqueRandomGenerator, count);

            uniqueRandomGenerator.Next(0, count).Returns(1);
            int end = constraint.GetEnd();

            Assert.That(end, Is.EqualTo(1));
        }
    }
}
