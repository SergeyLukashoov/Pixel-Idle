// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("IZhSRj0o4V+GzZpSFbVXzFXmEyVgG5573vlunfXpJkGuH2LsB1A2XjmETr445YDsUneH7Dv3XzO7g4NkcYqZ0NdiohXgVh3V6Y7qblnaXHbh9TCN/DAO/bY1aNOgW66TqiuAfn7MT2x+Q0hHZMgGyLlDT09PS05Nun6xBUs3sd5aE0qQ/kppPFodXNJPlmOuWW4g6MAFCb54vP5lrxm3lR+3aEqmdlnSk723EXqJ7VeDIZQsIXApSrmLBbU1YOcBWqE/8SsaLO7MT0FOfsxPREzMT09Ov3IZq3lTNMBQRpYJ7pSTeTpXLfKCFDZ1plMWEes6JiV5t9o3L0peJHk7w0UD0Vdv6QSbHYhE8aYHUVxtaHJS8GosAPPb0YCmK8Nr30xNT05P");
        private static int[] order = new int[] { 12,3,6,13,11,12,7,11,11,9,13,13,13,13,14 };
        private static int key = 78;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
