﻿using System;
using System.Linq;
using NUnit.Framework;
using SampleWebApp.Infrastructure;
using Tests.Helpers;

namespace Tests.UnitTests.Group06Mvc
{
    class Test02Validation
    {

        [Test]
        public void Check01TestModelValidate()
        {
            //SETUP  
            var model = new ModelStateTester.TestModel("123", 50, true);

            //ATTEMPT
            var vErrors = model.Validate(null).ToList();

            //VERIFY
            vErrors.Count.ShouldEqual(2);
            vErrors[0].MemberNames.Any().ShouldEqual(false);
            vErrors[0].ErrorMessage.ShouldEqual("This is a top level error caused by CreateValidationError being set.");
            vErrors[1].MemberNames.Any().ShouldEqual(false);
            vErrors[1].ErrorMessage.ShouldEqual("This is a top level error caused by MyInt having value 50.");
        }


        [Test]
        public void Check05TestModelStateValidateOnly()
        {
            //SETUP  
            var model = new ModelStateTester.TestModel("123", 50, true);

            //ATTEMPT
            var modelState = model.ReturnModelState();

            //VERIFY
            modelState.IsValid.ShouldEqual(false);
            modelState.Keys.Count.ShouldEqual(1);
            modelState.Keys.First().ShouldEqual("");
            modelState[modelState.Keys.First()].Errors.Count.ShouldEqual(2);
            modelState[modelState.Keys.First()].Errors[0].ErrorMessage.ShouldEqual("This is a top level error caused by CreateValidationError being set.");
            modelState[modelState.Keys.First()].Errors[1].ErrorMessage.ShouldEqual("This is a top level error caused by MyInt having value 50.");
        }

        [Test]
        public void Check06TestModelStateValidateOneOnly()
        {
            //SETUP  
            var model = new ModelStateTester.TestModel("123", 2, true);

            //ATTEMPT
            var modelState = model.ReturnModelState();

            //VERIFY
            modelState.IsValid.ShouldEqual(false);
            modelState.Keys.Count.ShouldEqual(1);
            modelState.Keys.First().ShouldEqual("");
            modelState[modelState.Keys.First()].Errors.Count.ShouldEqual(1);
            modelState[modelState.Keys.First()].Errors[0].ErrorMessage.ShouldEqual("This is a top level error caused by CreateValidationError being set.");
        }

        [Test]
        public void Check06TestModelStateIntAttributeOnly()
        {
            //SETUP  
            var model = new ModelStateTester.TestModel("123", -1, false);

            //ATTEMPT
            var modelState = model.ReturnModelState();

            //VERIFY
            modelState.IsValid.ShouldEqual(false);
            modelState.Keys.Count.ShouldEqual(1);
            modelState.Keys.First().ShouldEqual("MyInt");
            modelState[modelState.Keys.First()].Errors.Count.ShouldEqual(1);
            modelState[modelState.Keys.First()].Errors[0].ErrorMessage.ShouldEqual("The field MyInt must be between 0 and 100.");

        }

        [Test]
        public void Check07TestModelStateStringAttributeOnly()
        {
            //SETUP  
            var model = new ModelStateTester.TestModel("", 2, false);

            //ATTEMPT
            var modelState = model.ReturnModelState();

            //VERIFY
            modelState.IsValid.ShouldEqual(false);
            modelState.Keys.Count.ShouldEqual(1);
            modelState.Keys.First().ShouldEqual("MyString");
            CollectionAssert.AreEquivalent(new[]
            {
                "The field MyString must be a string or array type with a minimum length of '2'.",
                "The MyString field is required."
            }, 
            modelState[modelState.Keys.First()].Errors.Select( x => x.ErrorMessage));


        }

        [Test]
        public void Check08TestModelStateMixedErrorsOnly()
        {
            //SETUP  
            var model = new ModelStateTester.TestModel("", -1, true);

            //ATTEMPT
            var modelState = model.ReturnModelState();

            //VERIFY
            modelState.IsValid.ShouldEqual(false);
            CollectionAssert.AreEquivalent(new[] { "MyInt", "MyString" }, modelState.Keys);     //Note: only runs Validate if no attribute errors
            modelState["MyInt"].Errors.Count.ShouldEqual(1);
            modelState["MyString"].Errors.Count.ShouldEqual(2);
        }

        //---------------------------------------------------------------------
        //now use to check ValidationHelper ReturnModelErrorsAsJson

        [Test]
        public void Check15TestModelStateValidateOnly()
        {
            //SETUP  
            var model = new ModelStateTester.TestModel("123", 50, true);

            //ATTEMPT
            var jsonResult = model.ReturnModelState().ReturnModelErrorsAsJson();

            //VERIFY
            var json = jsonResult.Data.SerialiseToJsonUsingJsonNet();
            json.ShouldEqual("{\"errorsDict\":{\"\":{\"errors\":[\"This is a top level error caused by CreateValidationError being set.\",\"This is a top level error caused by MyInt having value 50.\"]}}}");
        }

        [Test]
        public void Check16TestModelStateValidateOneOnly()
        {
            //SETUP  
            var model = new ModelStateTester.TestModel("123", 2, true);

            //ATTEMPT
            var jsonResult = model.ReturnModelState().ReturnModelErrorsAsJson();

            //VERIFY
            var json = jsonResult.Data.SerialiseToJsonUsingJsonNet();
            json.ShouldEqual("{\"errorsDict\":{\"\":{\"errors\":[\"This is a top level error caused by CreateValidationError being set.\"]}}}");
        }


        [Test]
        public void Check16TestModelStateIntAttributeOnly()
        {
            //SETUP  
            var model = new ModelStateTester.TestModel("123", -1, false);

            //ATTEMPT
            var jsonResult = model.ReturnModelState().ReturnModelErrorsAsJson();

            //VERIFY
            var json = jsonResult.Data.SerialiseToJsonUsingJsonNet();
            json.ShouldEqual("{\"errorsDict\":{\"MyInt\":{\"errors\":[\"The field MyInt must be between 0 and 100.\"]}}}");
        }

        [Test]
        public void Check17TestModelStateStringAttributeOnly()
        {
            //SETUP  
            var model = new ModelStateTester.TestModel("", 2, false);

            //ATTEMPT
            var jsonResult = model.ReturnModelState().ReturnModelErrorsAsJson();

            //VERIFY
            var json = jsonResult.Data.SerialiseToJsonUsingJsonNet();
            const string order1 = "{\"errorsDict\":{\"MyString\":{\"errors\":[\"The field MyString must be a string or array type with a minimum length of '2'.\",\"The MyString field is required.\"]}}}";
            const string order2 = "{\"errorsDict\":{\"MyString\":{\"errors\":[\"The MyString field is required.\",\"The field MyString must be a string or array type with a minimum length of '2'.\"]}}}";
            (json == order1 || json == order2).ShouldEqual(true);

        }

        [Test]
        public void Check18TestModelStateMixedErrorsOnly()
        {
            //SETUP  
            var model = new ModelStateTester.TestModel("", -1, true);

            //ATTEMPT
            var jsonResult = model.ReturnModelState().ReturnModelErrorsAsJson();

            //VERIFY
            var json = jsonResult.Data.SerialiseToJsonUsingJsonNet();
            const string order1 = "{\"errorsDict\":{\"MyString\":{\"errors\":[\"The field MyString must be a string or array type with a minimum length of '2'.\",\"The MyString field is required.\"]},";
            const string order2 = "{\"errorsDict\":{\"MyString\":{\"errors\":[\"The MyString field is required.\",\"The field MyString must be a string or array type with a minimum length of '2'.\"]},";
            const string part2 = "\"MyInt\":{\"errors\":[\"The field MyInt must be between 0 and 100.\"]}}}";
            (json == order1+part2 || json == order2+part2).ShouldEqual(true);
        }
    }
}