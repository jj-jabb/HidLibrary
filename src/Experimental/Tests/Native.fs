﻿namespace Tests

open System
open System.IO
open IO.Usb.Hid
open IO.Usb.Hid.Native
open xunit

module Native = 

    // ---------------------------- Common ----------------------------

    let GetFirstHidDevice () = 
        SetupApiAdapter.GetDevices HidAdapter.HidClass true 
                     |> Seq.filter (fun device -> device.Description.Contains("Keyboard") = false && device.Description.Contains("Mouse") = false) 
                     |> Seq.head

    // ---------------------------- SetupAPI ----------------------------

    [<Fact>]
    let SetupAPIDevicePathEnumeration () = 
        let totalConnected = SetupApiAdapter.GetDevicePaths HidAdapter.HidClass true |> Seq.length
        let total = SetupApiAdapter.GetDevicePaths HidAdapter.HidClass false |> Seq.length
        Assert.Greater(total, 0)
        Assert.Greater(total, totalConnected)

    [<Fact>]
    let SetupAPIDeviceEnumeration () = 
        let totalConnected = SetupApiAdapter.GetDevices HidAdapter.HidClass true |> Seq.length
        let total = SetupApiAdapter.GetDevices HidAdapter.HidClass false |> Seq.length
        Assert.Greater(total, 0)
        Assert.Greater(total, totalConnected)

    // ---------------------------- Kernel32 ----------------------------

    [<Fact>]
    let CreateFileHandleTest () =
        let device = GetFirstHidDevice ()
        let handle = Kernel32Adapter.OpenFile device.Path FileAccess.Read FileShare.ReadWrite FileMode.Open false
        handle.Close()
    
    [<Fact>]
    let CreateDeviceStreamTest () =
        let device = GetFirstHidDevice ()
        let stream = new DeviceStream(device.Path, FileAccess.Read, FileShare.ReadWrite, FileMode.Open, false)
        stream.Close()

    // ---------------------------- Hid ----------------------------

    [<Fact>]
    let HidDeviceId () = 
        Assert.AreEqual(new Guid("4d1e55b2-f16f-11cf-88cb-001111000030"), HidAdapter.HidClass)

    [<Fact>]
    let HidDeviceAttributes () = 
        let device = GetFirstHidDevice ()
        let handle = Kernel32Adapter.OpenFile device.Path FileAccess.Read FileShare.ReadWrite FileMode.Open false
        let attributes = HidAdapter.GetDeviceAttributes(handle)
        handle.Close()
        Assert.Greater(attributes.VenderId, 0)
        Assert.Greater(attributes.ProductId, 0)
        Assert.Greater(attributes.Version, 0)