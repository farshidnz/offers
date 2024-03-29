﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (https://www.specflow.org/).
//      SpecFlow Version:3.9.0.0
//      SpecFlow Generator Version:3.9.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace CashrewardsOffers.Application.AcceptanceTests.Features.Anz
{
    using TechTalk.SpecFlow;
    using System;
    using System.Linq;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "3.9.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("Anz Api Pagination")]
    public partial class AnzApiPaginationFeature
    {
        
        private TechTalk.SpecFlow.ITestRunner testRunner;
        
        private static string[] featureTags = ((string[])(null));
        
#line 1 "AnzApiPagination.feature"
#line hidden
        
        [NUnit.Framework.OneTimeSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-AU"), "Features/Anz", "Anz Api Pagination", "Ability to ask for a page size and page offset", ProgrammingLanguage.CSharp, featureTags);
            testRunner.OnFeatureStart(featureInfo);
        }
        
        [NUnit.Framework.OneTimeTearDownAttribute()]
        public virtual void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        [NUnit.Framework.SetUpAttribute()]
        public void TestInitialize()
        {
        }
        
        [NUnit.Framework.TearDownAttribute()]
        public void TestTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public void ScenarioInitialize(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioInitialize(scenarioInfo);
            testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<NUnit.Framework.TestContext>(NUnit.Framework.TestContext.CurrentContext);
        }
        
        public void ScenarioStart()
        {
            testRunner.OnScenarioStart();
        }
        
        public void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Given no page size or offset return all anz offers")]
        public void GivenNoPageSizeOrOffsetReturnAllAnzOffers()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Given no page size or offset return all anz offers", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 5
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 6
 testRunner.Given("API for ANZ is available", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
#line 7
 testRunner.When("auth token is valid", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
                TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                            "OfferId",
                            "Merchant.Id",
                            "IsFeatured",
                            "IsPremiumFeature"});
                table1.AddRow(new string[] {
                            "301",
                            "101",
                            "true",
                            "true"});
                table1.AddRow(new string[] {
                            "302",
                            "101",
                            "true",
                            "true"});
#line 8
 testRunner.Given("offer data change event", ((string)(null)), table1, "Given ");
#line hidden
#line 12
 testRunner.When("I send an ANZ query", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
                TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                            "Id",
                            "IsFeatured",
                            "IsExculsive"});
                table2.AddRow(new string[] {
                            "101-301",
                            "true",
                            "true"});
                table2.AddRow(new string[] {
                            "101-302",
                            "true",
                            "true"});
#line 13
 testRunner.Then("I should receive ANZ items", ((string)(null)), table2, "Then ");
#line hidden
#line 17
 testRunner.Then("I should recieve the following response data from the anz api TotalOffersCount = " +
                        "\'2\', PageOffersCount = \'2\', TotalPageCount = \'1\', PageNumber = \'1\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Given some page size or offset return anz offers")]
        public void GivenSomePageSizeOrOffsetReturnAnzOffers()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Given some page size or offset return anz offers", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 20
 this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 21
 testRunner.Given("API for ANZ is available", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
#line 22
 testRunner.When("auth token is valid", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
                TechTalk.SpecFlow.Table table3 = new TechTalk.SpecFlow.Table(new string[] {
                            "OfferId",
                            "Merchant.Id",
                            "IsFeatured",
                            "IsPremiumFeature"});
                table3.AddRow(new string[] {
                            "301",
                            "101",
                            "true",
                            "true"});
                table3.AddRow(new string[] {
                            "302",
                            "101",
                            "true",
                            "true"});
#line 23
 testRunner.Given("offer data change event", ((string)(null)), table3, "Given ");
#line hidden
#line 27
 testRunner.When("I send an ANZ query with OffersPerPage = \'99999\', PageNumber = \'1\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
                TechTalk.SpecFlow.Table table4 = new TechTalk.SpecFlow.Table(new string[] {
                            "Id",
                            "IsFeatured",
                            "IsExculsive"});
                table4.AddRow(new string[] {
                            "101-301",
                            "true",
                            "true"});
                table4.AddRow(new string[] {
                            "101-302",
                            "true",
                            "true"});
#line 28
 testRunner.Then("I should receive ANZ items", ((string)(null)), table4, "Then ");
#line hidden
#line 32
 testRunner.Then("I should recieve the following response data from the anz api TotalOffersCount = " +
                        "\'2\', PageOffersCount = \'2\', TotalPageCount = \'1\', PageNumber = \'1\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 33
 testRunner.When("I send an ANZ query with OffersPerPage = \'1\', PageNumber = \'1\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
                TechTalk.SpecFlow.Table table5 = new TechTalk.SpecFlow.Table(new string[] {
                            "Id",
                            "IsFeatured",
                            "IsExculsive"});
                table5.AddRow(new string[] {
                            "101-301",
                            "true",
                            "true"});
#line 34
 testRunner.Then("I should receive ANZ items", ((string)(null)), table5, "Then ");
#line hidden
#line 37
 testRunner.Then("I should recieve the following response data from the anz api TotalOffersCount = " +
                        "\'2\', PageOffersCount = \'1\', TotalPageCount = \'2\', PageNumber = \'1\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 38
 testRunner.When("I send an ANZ query with OffersPerPage = \'1\', PageNumber = \'2\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
                TechTalk.SpecFlow.Table table6 = new TechTalk.SpecFlow.Table(new string[] {
                            "Id",
                            "IsFeatured",
                            "IsExculsive"});
                table6.AddRow(new string[] {
                            "101-302",
                            "true",
                            "true"});
#line 39
 testRunner.Then("I should receive ANZ items", ((string)(null)), table6, "Then ");
#line hidden
#line 42
 testRunner.Then("I should recieve the following response data from the anz api TotalOffersCount = " +
                        "\'2\', PageOffersCount = \'1\', TotalPageCount = \'2\', PageNumber = \'2\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
                TechTalk.SpecFlow.Table table7 = new TechTalk.SpecFlow.Table(new string[] {
                            "OfferId",
                            "Merchant.Id",
                            "IsFeatured",
                            "IsPremiumFeature"});
                table7.AddRow(new string[] {
                            "303",
                            "102",
                            "true",
                            "true"});
                table7.AddRow(new string[] {
                            "304",
                            "103",
                            "true",
                            "true"});
                table7.AddRow(new string[] {
                            "305",
                            "104",
                            "true",
                            "true"});
                table7.AddRow(new string[] {
                            "306",
                            "105",
                            "true",
                            "true"});
                table7.AddRow(new string[] {
                            "307",
                            "106",
                            "true",
                            "true"});
#line 43
 testRunner.Given("offer data change event", ((string)(null)), table7, "Given ");
#line hidden
#line 50
 testRunner.When("I send an ANZ query with OffersPerPage = \'5\', PageNumber = \'1\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
                TechTalk.SpecFlow.Table table8 = new TechTalk.SpecFlow.Table(new string[] {
                            "Id",
                            "IsFeatured",
                            "IsExculsive"});
                table8.AddRow(new string[] {
                            "101-301",
                            "true",
                            "true"});
                table8.AddRow(new string[] {
                            "101-302",
                            "true",
                            "true"});
                table8.AddRow(new string[] {
                            "102-303",
                            "true",
                            "true"});
                table8.AddRow(new string[] {
                            "103-304",
                            "true",
                            "true"});
                table8.AddRow(new string[] {
                            "104-305",
                            "true",
                            "true"});
#line 51
 testRunner.Then("I should receive ANZ items", ((string)(null)), table8, "Then ");
#line hidden
#line 58
 testRunner.Then("I should recieve the following response data from the anz api TotalOffersCount = " +
                        "\'7\', PageOffersCount = \'5\', TotalPageCount = \'2\', PageNumber = \'1\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 59
 testRunner.When("I send an ANZ query with OffersPerPage = \'5\', PageNumber = \'2\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
                TechTalk.SpecFlow.Table table9 = new TechTalk.SpecFlow.Table(new string[] {
                            "Id",
                            "IsFeatured",
                            "IsExculsive"});
                table9.AddRow(new string[] {
                            "105-306",
                            "true",
                            "true"});
                table9.AddRow(new string[] {
                            "106-307",
                            "true",
                            "true"});
#line 60
 testRunner.Then("I should receive ANZ items", ((string)(null)), table9, "Then ");
#line hidden
#line 64
 testRunner.Then("I should recieve the following response data from the anz api TotalOffersCount = " +
                        "\'7\', PageOffersCount = \'2\', TotalPageCount = \'2\', PageNumber = \'2\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
                TechTalk.SpecFlow.Table table10 = new TechTalk.SpecFlow.Table(new string[] {
                            "OfferId",
                            "Merchant.Id"});
                table10.AddRow(new string[] {
                            "301",
                            "101"});
                table10.AddRow(new string[] {
                            "302",
                            "101"});
#line 65
 testRunner.Given("offer delete event", ((string)(null)), table10, "Given ");
#line hidden
#line 69
 testRunner.When("I send an ANZ query with OffersPerPage = \'3\', PageNumber = \'1\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
                TechTalk.SpecFlow.Table table11 = new TechTalk.SpecFlow.Table(new string[] {
                            "Id",
                            "IsFeatured",
                            "IsExculsive"});
                table11.AddRow(new string[] {
                            "102-303",
                            "true",
                            "true"});
                table11.AddRow(new string[] {
                            "103-304",
                            "true",
                            "true"});
                table11.AddRow(new string[] {
                            "104-305",
                            "true",
                            "true"});
#line 70
 testRunner.Then("I should receive ANZ items", ((string)(null)), table11, "Then ");
#line hidden
#line 75
 testRunner.Then("I should recieve the following response data from the anz api TotalOffersCount = " +
                        "\'5\', PageOffersCount = \'3\', TotalPageCount = \'2\', PageNumber = \'1\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 76
 testRunner.When("I send an ANZ query with OffersPerPage = \'3\', PageNumber = \'2\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
                TechTalk.SpecFlow.Table table12 = new TechTalk.SpecFlow.Table(new string[] {
                            "Id",
                            "IsFeatured",
                            "IsExculsive"});
                table12.AddRow(new string[] {
                            "105-306",
                            "true",
                            "true"});
                table12.AddRow(new string[] {
                            "106-307",
                            "true",
                            "true"});
#line 77
 testRunner.Then("I should receive ANZ items", ((string)(null)), table12, "Then ");
#line hidden
#line 81
 testRunner.Then("I should recieve the following response data from the anz api TotalOffersCount = " +
                        "\'5\', PageOffersCount = \'2\', TotalPageCount = \'2\', PageNumber = \'2\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
