using Bunit;
using EduSense.UI.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using TestContext = Bunit.TestContext;

namespace EduSense.UI.Test
{
    public class ConfirmDialogTests : TestContext
    {
        [Fact]
        

        public void Not_rendered_when_IsVisible_false()
        {
            // bekräfta att dialogen inte renderas när IsVisible är false
            var cut = RenderComponent<ConfirmDialog>(parameters => parameters
                .Add(p => p.IsVisible, false));

            Assert.Empty(cut.Markup.Trim());
        }

        [Fact]
      
        public void Renders_title_and_message_when_visible()
        {
            // bekräfta att dialogen renderas med titel och meddelande när IsVisible är true

            JSInterop.SetupVoid("eduSenseFocusTrap.trap", _ => true);
            var cut = RenderComponent<ConfirmDialog>(parameters => parameters
                .Add(p => p.IsVisible, true)
                .Add(p => p.Title, "Ta bort enkät")
                .Add(p => p.Message, "Är du säker?"));

            Assert.Contains("Ta bort enkät", cut.Markup);
            Assert.Contains("Är du säker?", cut.Markup);
        }

        [Fact]
        public void Confirm_button_invokes_OnConfirmed()
        {
            // bekräfta att OnConfirmed anropas när användaren klickar på bekräfta-knappen

            JSInterop.SetupVoid("eduSenseFocusTrap.trap", _ => true);
            var confirmed = false;
            var cut = RenderComponent<ConfirmDialog>(parameters => parameters
                .Add(p => p.IsVisible, true)
                .Add(p => p.OnConfirmed, EventCallback.Factory.Create(this, () => confirmed = true)));

            cut.Find("button.button-logout").Click();

            Assert.True(confirmed);
        }

        [Fact]
        public void Cancel_button_invokes_OnCancelled()
        {
            // bekräfta att OnCancelled anropas när användaren klickar på avbryt-knappen

            JSInterop.SetupVoid("eduSenseFocusTrap.trap", _ => true);
            var cancelled = false;
            var cut = RenderComponent<ConfirmDialog>(parameters => parameters
                .Add(p => p.IsVisible, true)
                .Add(p => p.OnCancelled, EventCallback.Factory.Create(this, () => cancelled = true)));

            cut.Find("button.btn-secondary").Click();

            Assert.True(cancelled);
        }

        [Fact]
        public void Escape_key_invokes_OnCancelled()
        {
            // bekräfta att OnCancelled anropas när användaren trycker på Escape-tangenten

            JSInterop.SetupVoid("eduSenseFocusTrap.trap", _ => true);
            var cancelled = false;
            var cut = RenderComponent<ConfirmDialog>(parameters => parameters
                .Add(p => p.IsVisible, true)
                .Add(p => p.OnCancelled, EventCallback.Factory.Create(this, () => cancelled = true)));

            cut.Find("div.modal").KeyDown(new KeyboardEventArgs { Key = "Escape" });

            Assert.True(cancelled);
        }
    }
}
