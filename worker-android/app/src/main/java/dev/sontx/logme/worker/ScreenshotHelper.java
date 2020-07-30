package dev.sontx.logme.worker;

import android.app.Activity;
import android.graphics.Bitmap;
import android.util.Log;
import android.view.View;

import java.io.ByteArrayOutputStream;

final class ScreenshotHelper {
    private ScreenshotHelper() {
    }

    public static byte[] takeScreenshot(Activity activity) {
        try {
            View v1 = activity.getWindow().getDecorView().getRootView();
            v1.setDrawingCacheEnabled(true);
            Bitmap bitmap = Bitmap.createBitmap(v1.getDrawingCache());
            v1.setDrawingCacheEnabled(false);

            ByteArrayOutputStream outputStream = new ByteArrayOutputStream();
            bitmap.compress(Bitmap.CompressFormat.PNG, 100, outputStream);
            return outputStream.toByteArray();
        } catch (Throwable e) {
            Log.e(ScreenshotHelper.class.getName(), "takeScreenshot", e);
        }
        return new byte[0];
    }
}
