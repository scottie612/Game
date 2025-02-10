using UnityEngine;

[CreateAssetMenu(fileName = "New Animation", menuName = "Animations/New Animation")]
public class CustomAnimation : ScriptableObject
{
    public Texture2D SpriteSheet;

    public int FramesPerSecond = 12;
    public Sprite[] AnimationFrames { get; private set; }
    public float AnimationDuration => AnimationFrames.Length / (float)FramesPerSecond;

    public bool ShouldLoop = true;

    public int SpriteWidth = 100;
    public int SpriteHeight = 100;


    public void Initialize()
    {
        AnimationFrames = ConvertToSpriteArray(SpriteSheet);
    }

    private Sprite[] ConvertToSpriteArray(Texture2D spriteSheet)
    {
        spriteSheet.filterMode = FilterMode.Point;
        spriteSheet.anisoLevel = 0;
        spriteSheet.wrapMode = TextureWrapMode.Clamp;

        int columns = spriteSheet.width / SpriteWidth;
        int rows = spriteSheet.height / SpriteHeight;

        Sprite[] returnSprites = new Sprite[columns * rows];

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                Rect spriteRect = new Rect(
                    x * SpriteWidth,
                    spriteSheet.height - (y + 1) * SpriteHeight,
                    SpriteWidth,
                    SpriteHeight
                    );

                returnSprites[y * columns + x] = Sprite.Create(spriteSheet, spriteRect, new Vector2(.5f, .5f), 100);
            }
        }

        return returnSprites;
    }
}