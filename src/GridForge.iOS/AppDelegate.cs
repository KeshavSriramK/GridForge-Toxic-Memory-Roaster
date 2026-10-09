using Foundation;
using UIKit;
using GridForge.Core.Engine;

namespace GridForge.iOS
{
    [Register("AppDelegate")]
    public class AppDelegate : UIApplicationDelegate
    {
        public override UIWindow? Window { get; set; }

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            GameApp game = new GameApp();
            game.Run();
            return true;
        }

        public override UISceneConfiguration GetConfiguration(UIApplication application, UISceneSession connectingSceneSession, UISceneConnectionOptions options)
        {
            return new UISceneConfiguration("Default Configuration", connectingSceneSession.Role);
        }

        public override void DidDiscardSceneSessions(UIApplication application, NSSet<UISceneSession> sceneSessions)
        {
        }
    }
}