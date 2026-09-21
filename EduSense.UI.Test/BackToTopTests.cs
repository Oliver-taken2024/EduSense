using Bunit;
using EduSense.UI.Components;
using TestContext = Bunit.TestContext;

namespace EduSense.UI.Test
{
    public class BackToTopTests : TestContext
    {
        [Fact]
        public void Not_rendered_before_scroll_threshold_is_passed()
        {
            // Knappen ska vara osynlig direkt vid render, innan JS hunnit rapportera scroll-läget.

            JSInterop.SetupVoid("eduSenseBackToTop.init", _ => true);

            var cut = RenderComponent<BackToTop>();

            Assert.Empty(cut.Markup.Trim());
        }

        [Fact]
        public void Becomes_visible_when_OnScroll_reports_past_threshold()
        {
            // Simulerar att JS-sidan anropar tillbaka via [JSInvokable] efter att
            // användaren scrollat förbi tröskeln.

            JSInterop.SetupVoid("eduSenseBackToTop.init", _ => true);
            var cut = RenderComponent<BackToTop>();

            cut.InvokeAsync(() => cut.Instance.OnScroll(true));

            Assert.Contains("back-to-top", cut.Markup);
        }

        [Fact]
        public void Becomes_hidden_again_when_OnScroll_reports_back_above_threshold()
        {
            JSInterop.SetupVoid("eduSenseBackToTop.init", _ => true);
            var cut = RenderComponent<BackToTop>();

            cut.InvokeAsync(() => cut.Instance.OnScroll(true));
            cut.InvokeAsync(() => cut.Instance.OnScroll(false));

            Assert.Empty(cut.Markup.Trim());
        }

        [Fact]
        public void Clicking_the_button_calls_scrollToTop_in_JS()
        {
            JSInterop.SetupVoid("eduSenseBackToTop.init", _ => true);
            JSInterop.SetupVoid("eduSenseBackToTop.scrollToTop");
            var cut = RenderComponent<BackToTop>();
            cut.InvokeAsync(() => cut.Instance.OnScroll(true));

            cut.Find("button.back-to-top").Click();

            JSInterop.VerifyInvoke("eduSenseBackToTop.scrollToTop");
        }
    }
}
