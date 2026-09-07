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
            var cut = RenderComponent<ConfirmDialog>(parameters => parameters
                .Add(p => p.IsVisible, false));

            Assert.Empty(cut.Markup.Trim());
        }

        [Fact]
        public void Renders_title_and_message_when_visible()
        {
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
            var confirmed = false;
            var cut = RenderComponent<ConfirmDialog>(parameters => parameters
                .Add(p => p.IsVisible, true)
                .Add(p => p.OnConfirmed, EventCallback.Factory.Create(this, () => confirmed = true)));

            cut.Find("button.btn-danger").Click();

            Assert.True(confirmed);
        }

        [Fact]
        public void Cancel_button_invokes_OnCancelled()
        {
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
            var cancelled = false;
            var cut = RenderComponent<ConfirmDialog>(parameters => parameters
                .Add(p => p.IsVisible, true)
                .Add(p => p.OnCancelled, EventCallback.Factory.Create(this, () => cancelled = true)));

            cut.Find("div.modal").KeyDown(new KeyboardEventArgs { Key = "Escape" });

            Assert.True(cancelled);
        }
    }
}
