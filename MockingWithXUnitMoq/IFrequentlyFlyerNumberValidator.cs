namespace MockingWithXUnitMoq
{
    public interface IFrequentlyFlyerNumberValidator
    {
        public ValidationMode ValidationMode { get; set; }
        public IServiceInformation ServiceInformation { get; }
        bool IsValid(string frequentlyFlyerNumber);
        void IsValid(string frequentlyFlyerNumber, out bool isValid);
        event EventHandler ValidatorLookupPerformed;
    }

    public interface ILicenseData
    {
        public string LicenseKey { get; }
    }

    public interface IServiceInformation
    {
        public ILicenseData License { get; }
    }
}
