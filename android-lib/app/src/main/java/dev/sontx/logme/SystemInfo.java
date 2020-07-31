package dev.sontx.logme;

import android.content.Context;

import java.text.DecimalFormat;

import github.nisrulz.easydeviceinfo.base.EasyAppMod;
import github.nisrulz.easydeviceinfo.base.EasyBluetoothMod;
import github.nisrulz.easydeviceinfo.base.EasyConfigMod;
import github.nisrulz.easydeviceinfo.base.EasyDeviceMod;
import github.nisrulz.easydeviceinfo.base.EasyDisplayMod;
import github.nisrulz.easydeviceinfo.base.EasyMemoryMod;
import github.nisrulz.easydeviceinfo.base.EasyNetworkMod;

final class SystemInfo {
    private SystemInfo() {
    }

    public static String getInfo(Context context) {
        EasyConfigMod easyConfigMod = new EasyConfigMod(context);
        EasyDeviceMod easyDeviceMod = new EasyDeviceMod(context);
        EasyDisplayMod easyDisplayMod = new EasyDisplayMod(context);
        EasyMemoryMod easyMemoryMod = new EasyMemoryMod(context);
        EasyNetworkMod easyNetworkMod = new EasyNetworkMod(context);
        EasyBluetoothMod easyBluetoothMod = new EasyBluetoothMod(context);
        EasyAppMod easyAppMod = new EasyAppMod(context);

        final String divider = "----------------------------------------";

        StringBuilderEx builder = new StringBuilderEx();
        builder.appendLine("Device mode", easyConfigMod.isRunningOnEmulator() ? "Emulator" : "Real Phone");
        builder.appendLine("Android version", easyDeviceMod.getOSVersion());
        builder.appendLine("Fingerprint", easyDeviceMod.getFingerprint());
        builder.appendLine(divider);
        builder.appendLine("Display resolution", easyDisplayMod.getResolution());
        builder.appendLine("RAM", readableFileSize(easyMemoryMod.getTotalRAM()));
        builder.appendLine("Storage", getInternalStorageSize(easyMemoryMod));
        builder.appendLine("IPv4", easyNetworkMod.getIPv4Address());
        builder.appendLine("Language", easyDeviceMod.getLanguage());
        builder.appendLine(divider);
        builder.appendLine("Has SD card", easyConfigMod.hasSdCard() + (easyConfigMod.hasSdCard() ? getSdCardSize(easyMemoryMod) : ""));
        builder.appendLine("Has Bluetooth LE", easyBluetoothMod.hasBluetoothLe());
        builder.appendLine("Has Bluetooth LE Advertising", easyBluetoothMod.hasBluetoothLeAdvertising());
        builder.appendLine(divider);
        builder.appendLine("App name", String.format("%s (%s)", easyAppMod.getAppName(), easyAppMod.getAppVersion()));
        builder.appendLine("Package name", easyAppMod.getPackageName());

        return builder.toString();
    }

    private static String getSdCardSize(EasyMemoryMod easyMemoryMod) {
        return String.format(" (%s/%s)", readableFileSize(easyMemoryMod.getAvailableExternalMemorySize()), readableFileSize(easyMemoryMod.getTotalExternalMemorySize()));
    }

    private static String getInternalStorageSize(EasyMemoryMod easyMemoryMod) {
        return String.format("%s/%s", readableFileSize(easyMemoryMod.getAvailableInternalMemorySize()), readableFileSize(easyMemoryMod.getTotalInternalMemorySize()));
    }

    private static String readableFileSize(long size) {
        if (size <= 0) return "0";
        final String[] units = new String[]{"B", "kB", "MB", "GB", "TB"};
        int digitGroups = (int) (Math.log10(size) / Math.log10(1024));
        return new DecimalFormat("#,##0.#").format(size / Math.pow(1024, digitGroups)) + " " + units[digitGroups];
    }

    private static class StringBuilderEx {
        private final StringBuilder builder;

        public StringBuilderEx() {
            builder = new StringBuilder();
        }

        public void appendLine(String line) {
            builder.append(line).append('\n');
        }

        public void appendLine(String label, Object value) {
            builder.append(String.format("%s: %s\n", label, value.toString()));
        }

        @Override
        public String toString() {
            return builder.toString();
        }
    }
}
