public static class CellGestureMath
{
    public static float RotationFromDrag(float horizontalPixels, float degreesPerPixel)
    {
        return -horizontalPixels * degreesPerPixel;
    }

    public static float HeightAboveFloor(float floorHeight, float separation)
    {
        return floorHeight + System.Math.Max(0f, separation);
    }

    public static float NextScale(float currentScale, float previousDistance, float currentDistance, float minimumScale, float maximumScale)
    {
        if (previousDistance <= 0f)
            return currentScale;

        float scaledValue = currentScale * (currentDistance / previousDistance);
        return System.Math.Clamp(scaledValue, minimumScale, maximumScale);
    }
}
