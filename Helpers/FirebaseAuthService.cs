namespace PinoyMassageService.Helpers
{
    using FirebaseAdmin;
    using FirebaseAdmin.Auth;
    using Google.Apis.Auth;
    using Google.Apis.Auth.OAuth2;

    public sealed class FirebaseAuthService
    {
        private static readonly Lazy<FirebaseAuthService> lazy =
            new Lazy<FirebaseAuthService>(() => new FirebaseAuthService());

        public static FirebaseAuthService Instance { get { return lazy.Value; } }

        private FirebaseAuthService()
        {
            // Initialize the Firebase Admin SDK
            //string path = @"path\to\serviceAccountKey.json"; // replace with the actual path
            string path = Path.Combine(Directory.GetCurrentDirectory(), "pinoy-massage-app-firebase-adminsdk-vymll-b5f8c2d2e9.json");
            var cred = GoogleCredential.FromFile(path);
            FirebaseApp.Create(new AppOptions() { Credential = cred });
        }

        public async Task<string> GetUserUid(string idToken)
        {
            try
            {
                // Verify the ID token
                var token = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);

                // Get the user's UID
                return token.Uid;
            }
            catch (FirebaseAuthException ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
