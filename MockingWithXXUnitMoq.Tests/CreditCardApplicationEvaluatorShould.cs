using MockingWithXUnitMoq;
using Moq;
using Moq.Protected;

namespace MockingWithXXUnitMoq.Tests
{
    public class CreditCardApplicationEvaluatorShould
    {
        private Mock<IFrequentlyFlyerNumberValidator> mockValidator;
        private CreditCardApplicationEvaluator sut;

        public CreditCardApplicationEvaluatorShould()
        {
            mockValidator = new Mock<IFrequentlyFlyerNumberValidator>();
            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("Ok");
            mockValidator.Setup(x => x.IsValid(It.IsAny<string>())).Returns(true);

            sut = new CreditCardApplicationEvaluator(mockValidator.Object);
        }

        #region state
        [Fact]
        public void AcceptHighIncomeApplications()
        {
            var application = new CreditCardApplication { GrossAnnualIncome = 100 };

            var decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.AutoAccepted, decision);
        }

        [Fact]
        public void ReferYoungApplications()
        {
            mockValidator.DefaultValue = DefaultValue.Mock;

            var application = new CreditCardApplication { Age = 19 };

            var decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }

        [Fact]
        public void DeclineLowIncomeApplication()
        {
            mockValidator.DefaultValue = DefaultValue.Mock;

            var application = new CreditCardApplication()
            {
                GrossAnnualIncome = 19,
                Age = 42,
                FrequentlyFlyerNumber = "x"
            };

            var decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.AutoDeclined, decision);
        }

        [Fact]
        public void DeclineLowIncomeApplicationsOutDemo()
        {
            var mockValidator = new Mock<IFrequentlyFlyerNumberValidator>(MockBehavior.Strict);
            bool isValid = true;
            mockValidator.SetupAllProperties();
            mockValidator.Setup(_ => _.IsValid(It.IsAny<string>(), out isValid));

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

            var application = new CreditCardApplication()
            {
                GrossAnnualIncome = 19,
                Age = 42
            };

            var decision = sut.EvaluateUsingOut(application);

            Assert.Equal(CreditCardApplicationDecision.AutoDeclined, decision);
        }

        [Fact]
        public void ReferWhenLicenseKeyExpired()
        {
            //var mockLicenseData = new Mock<ILicenseData>();
            //mockLicenseData.Setup(_ => _.LicenseKey).Returns(GetLicenseKeyExpiryString);

            //var mockServiceInfo = new Mock<IServiceInformation>();
            //mockServiceInfo.Setup(_ => _.License).Returns(mockLicenseData.Object);

            var mockValidator = new Mock<IFrequentlyFlyerNumberValidator>();
            mockValidator.Setup(_ => _.IsValid(It.IsAny<string>())).Returns(true);
            mockValidator
                .Setup(_ => _.ServiceInformation.License.LicenseKey)
                .Returns(GetLicenseKeyExpiryString);

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

            var application = new CreditCardApplication { Age = 42 };

            var decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.AutoDeclined, decision);
        }
        #endregion

        #region behavior
        [Fact]
        public void ValidateFrequentlyFlyerNumberForLowIncomeApplications()
        {
            var application = new CreditCardApplication
            {
                FrequentlyFlyerNumber = "q"
            };

            sut.Evaluate(application);

            mockValidator.Verify(x => x.IsValid(It.IsAny<string>()), "Frequent flyer shuold be validated.");
        }

        [Fact]
        public void NotValidateFrequentFlyerNumberForHighIncomeApplications()
        {
            var application = new CreditCardApplication
            {
                GrossAnnualIncome = 10
            };

            sut.Evaluate(application);

            mockValidator.Verify(x => x.IsValid(It.IsAny<string>()), Times.Once);
            mockValidator.Verify(x => x.IsValid(It.IsAny<string>()), Times.Exactly(1));
        }

        [Fact]
        public void CheckLicenseKeyForLowIncomeApplications()
        {
            var application = new CreditCardApplication { GrossAnnualIncome = 99 };

            sut.Evaluate(application);

            mockValidator.VerifyGet(x => x.ServiceInformation.License.LicenseKey, Times.Never);
        }

        [Fact]
        public void SetDetailedLookupForOlderApplications()
        {
            var application = new CreditCardApplication { Age = 30 };

            sut.Evaluate(application);

            mockValidator.VerifySet(x => x.ValidationMode = ValidationMode.Detailed);
        }
        #endregion

        [Fact]
        public void ReferWhenFrequentlyFlyerValidatioError()
        {
            //mockValidator.Setup(x => x.IsValid(It.IsAny<string>())).Throws(new Exception("custom message"));  if i want to throw custom exception
            mockValidator.Setup(x => x.IsValid(It.IsAny<string>())).Throws<Exception>();

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

            var application = new CreditCardApplication { Age = 42 };

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }

        //Events from Mock Objects
        [Fact]
        public void IncremenetLookupCount()
        {
            mockValidator.Setup(x => x.IsValid(It.IsAny<string>()))
                .Returns(true)
                .Raises(x => x.ValidatorLookupPerformed += null, EventArgs.Empty);

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

            var application = new CreditCardApplication { FrequentlyFlyerNumber = "x", Age = 25 };

            sut.Evaluate(application);

            //mockValidator.Raise(x => x.ValidatorLookupPerformed += null, EventArgs.Empty);

            Assert.Equal(1, sut.ValidatorLookupCount);
        }

        [Fact]
        public void ReferInvalidFrequentlyFlyerApplications_ReturnValueSequence()
        {
            mockValidator.SetupSequence(x => x.IsValid(It.IsAny<string>()))
                .Returns(false)
                .Returns(true);

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

            var application = new CreditCardApplication { Age = 25 };

            CreditCardApplicationDecision firstDecision = sut.Evaluate(application);
            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, firstDecision);

            CreditCardApplicationDecision secondDecision = sut.Evaluate(application);
            Assert.Equal(CreditCardApplicationDecision.AutoDeclined, secondDecision);
        }

        [Fact]
        public void ReferInvalidFrequentlyFlyerApplications_MultipleCallsSequence()
        {
            var frequentlyFlyerNumberPassed = new List<string>();
            mockValidator.Setup(x => x.IsValid(Capture.In(frequentlyFlyerNumberPassed)));

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

            var application1 = new CreditCardApplication { Age = 25, FrequentlyFlyerNumber = "aa" };
            var application2 = new CreditCardApplication { Age = 25, FrequentlyFlyerNumber = "bb" };
            var application3 = new CreditCardApplication { Age = 25, FrequentlyFlyerNumber = "cc" };

            sut.Evaluate(application1);
            sut.Evaluate(application2);
            sut.Evaluate(application3);

            Assert.Equal(new List<string>() { "aa", "bb", "cc" }, frequentlyFlyerNumberPassed);
        }

        [Fact]
        public void ReferFraudRisk()
        {
            Mock<FraudLookup> mockFraudLookup = new Mock<FraudLookup>(); //not mocking an interface, the method should be virtual

            //mockFraudLookup.Setup(x => x.IsFraudRisk(It.IsAny<CreditCardApplication>())).Returns(true);
            
            //using protecteds
            mockFraudLookup
                .Protected()
                .Setup<bool>("CheckApplication", ItExpr.IsAny<CreditCardApplication>())
                .Returns(true);

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object, mockFraudLookup.Object);

            var application = new CreditCardApplication();

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.ReferredToHumanFraudRisk, decision);
        }

        [Fact]
        public void LinqToMocks()
        {
            IFrequentlyFlyerNumberValidator mockValidator = Mock.Of<IFrequentlyFlyerNumberValidator>
            (
                validator =>
                validator.ServiceInformation.License.LicenseKey == "OK" &&
                validator.IsValid(It.IsAny<string>()) == true
            );

            var sut = new CreditCardApplicationEvaluator(mockValidator);

            var application = new CreditCardApplication { Age = 25 };

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.AutoDeclined, decision);
        }

        //executed at the time the prop is accessed
        string GetLicenseKeyExpiryString()
        {
            return "Expired";
        }
    }
}