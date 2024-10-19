namespace MockingWithXUnitMoq
{
    public class FrequentlyFlyerNumberValidator : IFrequentlyFlyerNumberValidator
    {
        public IServiceInformation ServiceInformation =>
            throw new NotImplementedException("For demo purposes.");

        public ValidationMode ValidationMode { get; set; } = ValidationMode.None;

        public event EventHandler ValidatorLookupPerformed;

        public bool IsValid(string frequentlyFlyerNumber)
        {
            throw new NotImplementedException("Simulate this real dependency being hard to use");
        }

        public void IsValid(string frequentlyFlyerNumber, out bool isValid)
        {
            throw new NotImplementedException("Simulate this real dependency being hard to use");
        }
    }
}
