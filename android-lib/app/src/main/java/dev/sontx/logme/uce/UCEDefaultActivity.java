package dev.sontx.logme.uce;

import android.annotation.SuppressLint;
import android.app.Activity;
import android.app.AlertDialog;
import android.content.ClipData;
import android.content.ClipboardManager;
import android.content.Context;
import android.content.Intent;
import android.content.pm.ApplicationInfo;
import android.content.pm.PackageInfo;
import android.content.pm.PackageManager;
import android.net.Uri;
import android.os.Build;
import android.os.Bundle;
import android.os.Environment;
import android.text.TextUtils;
import android.util.Log;
import android.util.TypedValue;
import android.widget.TextView;
import android.widget.Toast;

import androidx.core.content.FileProvider;

import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.text.DateFormat;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.Locale;

import dev.sontx.logme.R;

/**
 * Created by Rohit.
 */
public final class UCEDefaultActivity extends Activity {
    private File txtFile;

    @SuppressLint("PrivateResource")
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        setTheme(android.R.style.Theme_Holo_Light_DarkActionBar);
        super.onCreate(savedInstanceState);
        setContentView(R.layout.default_error_activity);
        findViewById(R.id.button_close_app).setOnClickListener(v -> UCEDefaultActivity.this.finish());
        findViewById(R.id.button_copy_error_log).setOnClickListener(v -> copyErrorToClipboard());
        findViewById(R.id.button_share_error_log).setOnClickListener(v -> shareErrorLog());
        findViewById(R.id.button_save_error_log).setOnClickListener(v -> saveErrorLogToFile(true));
        findViewById(R.id.button_email_error_log).setOnClickListener(v -> emailErrorLog());
        findViewById(R.id.button_view_error_log).setOnClickListener(v -> {
            AlertDialog dialog = new AlertDialog.Builder(UCEDefaultActivity.this)
                    .setTitle("Error Log")
                    .setMessage(getAllErrorDetailsFromIntent(UCEDefaultActivity.this, getIntent()))
                    .setPositiveButton("Copy Log & Close",
                            (dialog1, which) -> {
                                copyErrorToClipboard();
                                dialog1.dismiss();
                            })
                    .setNeutralButton("Close",
                            (dialog12, which) -> dialog12.dismiss())
                    .show();
            TextView textView = dialog.findViewById(android.R.id.message);
            if (textView != null) {
                textView.setTextSize(TypedValue.COMPLEX_UNIT_SP, 18);
            }
        });
    }

    @Override
    protected void onDestroy() {
        super.onDestroy();
        UCEHandler.killCurrentProcess();
    }

    public static String getApplicationName(Context context) {
        ApplicationInfo applicationInfo = context.getApplicationInfo();
        int stringId = applicationInfo.labelRes;
        return stringId == 0 ? applicationInfo.nonLocalizedLabel.toString() : context.getString(stringId);
    }

    private static String getVersionName(Context context) {
        try {
            PackageInfo packageInfo = context.getPackageManager().getPackageInfo(context.getPackageName(), 0);
            return packageInfo.versionName;
        } catch (Exception e) {
            return "Unknown";
        }
    }

    private static String getActivityLogFromIntent(Intent intent) {
        String value = intent.getStringExtra(UCEHandler.EXTRA_ACTIVITY_LOG);
        return value != null ? value.trim() : null;
    }

    private static String getStackTraceFromIntent(Intent intent) {
        String value = intent.getStringExtra(UCEHandler.EXTRA_STACK_TRACE);
        return value != null ? value.trim() : null;
    }

    private void emailErrorLog() {
        saveErrorLogToFile(false);
        String errorLog = getAllErrorDetailsFromIntent(UCEDefaultActivity.this, getIntent());
        String[] emailAddressArray = UCEHandler.COMMA_SEPARATED_EMAIL_ADDRESSES.trim().split("\\s*,\\s*");
        Intent emailIntent = new Intent(android.content.Intent.ACTION_SEND);
        emailIntent.setType("plain/text");
        emailIntent.putExtra(Intent.EXTRA_EMAIL, emailAddressArray);
        emailIntent.putExtra(Intent.EXTRA_SUBJECT, getApplicationName(UCEDefaultActivity.this) + " Application Crash Error Log");
        emailIntent.putExtra(Intent.EXTRA_TEXT, getString(R.string.email_welcome_note) + errorLog);
        if (txtFile.exists()) {
            Uri filePath = FileProvider.getUriForFile(this, this.getApplicationContext().getPackageName() + ".provider", txtFile);
            emailIntent.putExtra(Intent.EXTRA_STREAM, filePath);
        }
        emailIntent.addFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION);
        startActivity(Intent.createChooser(emailIntent, "Email Error Log"));
    }

    private void saveErrorLogToFile(boolean isShowToast) {
        Boolean isSDPresent = Environment.getExternalStorageState().equals(Environment.MEDIA_MOUNTED);
        if (isSDPresent && isExternalStorageWritable()) {
            Date currentDate = new Date();
            DateFormat dateFormat = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss", Locale.US);
            String strCurrentDate = dateFormat.format(currentDate);
            strCurrentDate = strCurrentDate.replace(" ", "_");
            String errorLogFileName = getApplicationName(UCEDefaultActivity.this) + "_Error-Log_" + strCurrentDate;
            String errorLog = getAllErrorDetailsFromIntent(UCEDefaultActivity.this, getIntent());
            String fullPath = Environment.getExternalStorageDirectory() + "/AppErrorLogs_UCEH/";
            FileOutputStream outputStream;
            try {
                File file = new File(fullPath);
                file.mkdir();
                txtFile = new File(fullPath + errorLogFileName + ".txt");
                txtFile.createNewFile();
                outputStream = new FileOutputStream(txtFile);
                outputStream.write(errorLog.getBytes());
                outputStream.close();
                if (txtFile.exists() && isShowToast) {
                    Toast.makeText(this, "File Saved Successfully", Toast.LENGTH_SHORT).show();
                }
            } catch (IOException e) {
                Log.e("REQUIRED", "This app does not have write storage permission to save log file.");
                if (isShowToast) {
                    Toast.makeText(this, "Storage Permission Not Found", Toast.LENGTH_SHORT).show();
                }
                e.printStackTrace();
            }
        }
    }

    private void shareErrorLog() {
        String errorLog = getAllErrorDetailsFromIntent(UCEDefaultActivity.this, getIntent());
        Intent share = new Intent(Intent.ACTION_SEND);
        share.setType("text/plain");
        share.addFlags(Intent.FLAG_ACTIVITY_CLEAR_WHEN_TASK_RESET);
        share.putExtra(Intent.EXTRA_SUBJECT, "Application Crash Error Log");
        share.putExtra(Intent.EXTRA_TEXT, errorLog);
        startActivity(Intent.createChooser(share, "Share Error Log"));
    }

    private void copyErrorToClipboard() {
        String errorInformation = getAllErrorDetailsFromIntent(UCEDefaultActivity.this, getIntent());
        ClipboardManager clipboard = (ClipboardManager) getSystemService(CLIPBOARD_SERVICE);
        if (clipboard != null) {
            ClipData clip = ClipData.newPlainText("View Error Log", errorInformation);
            clipboard.setPrimaryClip(clip);
            Toast.makeText(UCEDefaultActivity.this, "Error Log Copied", Toast.LENGTH_SHORT).show();
        }
    }

    public static String getAllErrorDetailsFromIntent(Context context, Intent intent) {
        String LINE_SEPARATOR = "\n";
        StringBuilder errorReport = new StringBuilder();
        errorReport.append("\n***** DEVICE INFO \n");
        errorReport.append("Brand: ");
        errorReport.append(Build.BRAND);
        errorReport.append(LINE_SEPARATOR);
        errorReport.append("Device: ");
        errorReport.append(Build.DEVICE);
        errorReport.append(LINE_SEPARATOR);
        errorReport.append("Model: ");
        errorReport.append(Build.MODEL);
        errorReport.append(LINE_SEPARATOR);
        errorReport.append("Manufacturer: ");
        errorReport.append(Build.MANUFACTURER);
        errorReport.append(LINE_SEPARATOR);
        errorReport.append("Product: ");
        errorReport.append(Build.PRODUCT);
        errorReport.append(LINE_SEPARATOR);
        errorReport.append("SDK: ");
        errorReport.append(Build.VERSION.SDK);
        errorReport.append(LINE_SEPARATOR);
        errorReport.append("Release: ");
        errorReport.append(Build.VERSION.RELEASE);
        errorReport.append(LINE_SEPARATOR);
        errorReport.append("\n***** APP INFO \n");
        String versionName = getVersionName(context);
        errorReport.append("Version: ");
        errorReport.append(versionName);
        errorReport.append(LINE_SEPARATOR);
        Date currentDate = new Date();
        DateFormat dateFormat = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss", Locale.US);
        String firstInstallTime = getFirstInstallTimeAsString(context, dateFormat);
        if (!TextUtils.isEmpty(firstInstallTime)) {
            errorReport.append("Installed On: ");
            errorReport.append(firstInstallTime);
            errorReport.append(LINE_SEPARATOR);
        }
        String lastUpdateTime = getLastUpdateTimeAsString(context, dateFormat);
        if (!TextUtils.isEmpty(lastUpdateTime)) {
            errorReport.append("Updated On: ");
            errorReport.append(lastUpdateTime);
            errorReport.append(LINE_SEPARATOR);
        }
        errorReport.append("Current Date: ");
        errorReport.append(dateFormat.format(currentDate));
        errorReport.append(LINE_SEPARATOR);
        errorReport.append("\n***** ERROR LOG \n");
        errorReport.append(getStackTraceFromIntent(intent));
        errorReport.append(LINE_SEPARATOR);
        String activityLog = getActivityLogFromIntent(intent);
        errorReport.append(LINE_SEPARATOR);
        if (activityLog != null) {
            errorReport.append("\n***** USER ACTIVITIES \n");
            errorReport.append("User Activities: ");
            errorReport.append(activityLog);
            errorReport.append(LINE_SEPARATOR);
        }
        errorReport.append("\n***** END OF LOG *****\n");
        return errorReport.toString();
    }

    private static String getFirstInstallTimeAsString(Context context, DateFormat dateFormat) {
        long firstInstallTime;
        try {
            firstInstallTime = context
                    .getPackageManager()
                    .getPackageInfo(context.getPackageName(), 0)
                    .firstInstallTime;
            return dateFormat.format(new Date(firstInstallTime));
        } catch (PackageManager.NameNotFoundException e) {
            return "";
        }
    }

    private static String getLastUpdateTimeAsString(Context context, DateFormat dateFormat) {
        long lastUpdateTime;
        try {
            lastUpdateTime = context
                    .getPackageManager()
                    .getPackageInfo(context.getPackageName(), 0)
                    .lastUpdateTime;
            return dateFormat.format(new Date(lastUpdateTime));
        } catch (PackageManager.NameNotFoundException e) {
            return "";
        }
    }

    public boolean isExternalStorageWritable() {
        String state = Environment.getExternalStorageState();
        if (Environment.MEDIA_MOUNTED.equals(state)) {
            return true;
        }
        return false;
    }
}
