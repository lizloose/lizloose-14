using System.Numerics;
using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Prototypes;
namespace Content.Client._UM.UserInterface.Controls;

/// <summary>
/// Control for putting an outline around text. TODO: make the outline thickness configurable
/// </summary>
public sealed partial class OutlineRichTextLabel : RichTextLabel
{
    private static readonly ProtoId<ShaderPrototype> OutlinePrototype = "FontOutline";
    private Vector2 _textScaling = Vector2.One;
    private ShaderInstance? _outlineShader;
    public static int Thickness = 2;

    public OutlineRichTextLabel()
    {
        IoCManager.InjectDependencies(this);
        var prototypes = IoCManager.Resolve<IPrototypeManager>();
        _outlineShader = prototypes.Index(OutlinePrototype).InstanceUnique();
    }

    static List<Vector2> BuildOutlineOffsets()
    {
        var list = new List<Vector2>();

        for (var x = -Thickness; x <= Thickness; x++)
        {
            for (var y = -Thickness; y <= Thickness; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                list.Add(new Vector2(x, y));
            }
        }
        return list;
    }

    protected override Vector2 MeasureOverride(Vector2 availableSize)
    {
        var desiredTextSize = base.MeasureOverride(availableSize);
        var clampedScale = Vector2.Min(availableSize / desiredTextSize, Vector2.One);
        var keepAspectRatio = MathF.Min(clampedScale.X, clampedScale.Y);
        const float shimmerReduction = 0.1f;
        _textScaling = Vector2.One * MathF.Round(keepAspectRatio / shimmerReduction) * shimmerReduction;
        return desiredTextSize;
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        var offsets = BuildOutlineOffsets();

        handle.UseShader(_outlineShader);

        foreach (var o in offsets)
        {
            Vector2 pixelOffset = o;
            Vector2 scaledOffset = pixelOffset / _textScaling;

            handle.SetTransform(
                GlobalPixelPosition - PixelPosition + scaledOffset,
                0f,
                _textScaling
            );

            base.Draw(handle);
        }

        handle.UseShader(null);
        handle.SetTransform(
            GlobalPixelPosition - PixelPosition,
            0f,
            _textScaling
        );

        base.Draw(handle);

        handle.SetTransform(Matrix3x2.Identity);
        handle.UseShader(null);
    }

}

