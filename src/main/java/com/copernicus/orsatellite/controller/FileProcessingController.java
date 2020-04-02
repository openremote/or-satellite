package com.copernicus.orsatellite.controller;

import com.copernicus.orsatellite.service.FileProcessingImpl;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import java.io.File;
import java.io.IOException;
import java.util.concurrent.Executors;

@RestController
@RequestMapping("/api/fileprocessing")
public class FileProcessingController {

    boolean isWindows = System.getProperty("os.name").toLowerCase().startsWith("windows");

    @GetMapping("/testshell")
    public void TestShell2() throws IOException, InterruptedException {
        ProcessBuilder builder = new ProcessBuilder();
        if (isWindows) {
            builder.command("cmd.exe", "/c", "ipconfig", "/all");
        } else {
            builder.command("ls", "-ltrh");
        }
        builder.directory(new File(System.getProperty("user.home")));
        Process process = builder.start();
        FileProcessingImpl streamGobbler =
                new FileProcessingImpl(process.getInputStream(), System.out::println);
        Executors.newSingleThreadExecutor().submit(streamGobbler);
        int exitCode = process.waitFor();
        assert exitCode == 0;
    }
}
