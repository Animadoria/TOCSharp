
using System.Text;
using TOCSharp.Commands;
using TOCSharp.Commands.Attributes;

namespace ExampleCS;

public class RainbowCommand : ICommandModule
{
    [Command("rainbow")]
    public async Task Rainbow(CommandContext ctx, [RemainingText] string? text = null)
    {
        if (text == null)
        {
            await ctx.ReplyAsync($"What do you want me to {ConvertToRainbowText("rainbowify")}?");
            return;
        }
        string rainbow = ConvertToRainbowText(text);
        if (rainbow.Length > 1200)
        {
            await ctx.ReplyAsync($"Sorry, that's too {ConvertToRainbowText("long")}!");
            return;
        }
        await ctx.ReplyAsync(rainbow);
    }

    private static string ConvertToRainbowText(string input)
    {
        StringBuilder result = new();

        // Number of color steps in the gradient
        const int gradientSteps = 25;

        for (int i = 0; i < input.Length; i++)
        {
            char currentChar = input[i];
            string colorCode = GetGradientColor(i, gradientSteps);

            // Append the formatted string
            result.Append($"<font color=\"#{colorCode}\">{currentChar}</font>");
        }

        return result.ToString();
    }

    private static string GetGradientColor(int step, int totalSteps)
    {
        // Rainbow color spectrum (ROYGBIV)
        string[] rainbowColors = ["FF0000", "FF7F00", "FFFF00", "00FF00", "0000FF", "4B0082", "8B00FF"];

        // Ensure that the color index stays within bounds
        int colorIndex = step % (totalSteps * rainbowColors.Length) / totalSteps;

        return rainbowColors[colorIndex];
    }
}


