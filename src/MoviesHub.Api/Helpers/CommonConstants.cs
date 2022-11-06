using MoviesHub.Api.Models.Response.HubtelSms;

namespace MoviesHub.Api.Helpers;

public static class CommonConstants
{
    private const string MobileNumberIdentifier = "%mobileNumber%";
    private const string RequestIdIdentifier = "%requestId%";
    private const string PrefixIdentifier = "%prefix%";
    private const string OtpCodeIdentifier = "%otpCode%";
    private const string OtpCodeExpiryIdentifier = "%otpCodeExpiry";

    public static class Authentication
    {
        public const string AppAuthIdentity = "movieshub";
        private const string UserOtpKey = $"movieshub:otp:{MobileNumberIdentifier}:{RequestIdIdentifier}";

        private const string OtpSmsContent =
            $"Your authentication code for MoviesHub is {PrefixIdentifier}-{OtpCodeIdentifier}. " +
            $"Your OTP code expires in {OtpCodeExpiryIdentifier} minutes.";

        public static string GetUserOtpKey(string mobileNumber, string requestId) =>
            UserOtpKey.Replace(MobileNumberIdentifier, mobileNumber)
                .Replace(RequestIdIdentifier, requestId);

        public static string GetOtpSmsContent(SendSmsRequest request, int otpCodeExpiry) =>
            OtpSmsContent.Replace(PrefixIdentifier, request.Prefix)
                .Replace(OtpCodeIdentifier, request.Code.ToString())
                .Replace(OtpCodeExpiryIdentifier, otpCodeExpiry.ToString());
    }

    public static class Movies
    {
        public const string TopRatedMoviesPath = "movie/top_rated";
        public const string PopularMoviesPath = "movie/popular";
        public const string UpcomingMoviesPath = "movie/upcoming";
        public const string TrendingMoviesPath = "trending/all/day";
    }

    public static class User
    {
        private const string UserKey = $"movieshub:user:{MobileNumberIdentifier}";

        public static string GetUserKey(string mobileNumber) =>
            UserKey.Replace(MobileNumberIdentifier, mobileNumber);
    }
}