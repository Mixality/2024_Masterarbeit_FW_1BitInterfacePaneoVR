﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;



public class testWebGLtvOS : MonoBehaviour
{
#if UNITY_WEBGL || UNITY_TVOS

	//an output Buffer for the decompressed gz buffer
	private byte[] outbuffer = null, outbuffer2 = null;
	private Texture2D tex = null, tex2 = null, tex3 = null, tex4 = null, tex5 = null, tex6 = null, tex7 = null, tex8 = null, tex9 = null;

	byte[] wwb = null, wwb3 = null, zipwww = null;

	private bool downloadDone1, downloadDone2, downloadDone3, downloadDone4;

	private string log = "";

    //log for output of results
    void plog(string t = "")
    {
        log += t + "\n"; ;
    }

	void Start(){
		tex = new Texture2D(1,1,TextureFormat.RGBA32, false);
        tex2 = new Texture2D(1,1,TextureFormat.RGBA32, false);
        tex3 = new Texture2D(1,1,TextureFormat.RGBA32, false);
        tex4 = new Texture2D(1,1,TextureFormat.RGBA32, false);
        tex5 = new Texture2D(1,1,TextureFormat.RGBA32, false);
        tex6 = new Texture2D(1,1,TextureFormat.RGBA32, false);
        tex7 = new Texture2D(1,1,TextureFormat.RGBA32, false);
        tex8 = new Texture2D(1,1,TextureFormat.RGBA32, false);
        tex9 = new Texture2D(1,1,TextureFormat.RGBA32, false);

		//get some files from a server
		StartCoroutine( getFromSite() );


    }




    void OnGUI(){

		if(tex != null) GUI.DrawTexture(new Rect(200, 10, 185, 140), tex);
        if(tex2 != null) GUI.DrawTexture(new Rect(395, 10, 185, 140), tex2);
        if(tex3 != null) GUI.DrawTexture(new Rect(590, 10,185, 140), tex3);		
        if(tex4 != null) GUI.DrawTexture(new Rect(785, 10,185, 140), tex4);
        if(tex5 != null) GUI.DrawTexture(new Rect(980, 10,185, 140), tex5);
        if(tex6 != null) GUI.DrawTexture(new Rect(200, 160,125, 100), tex6);
        if(tex7 != null) GUI.DrawTexture(new Rect(335, 160,125, 100), tex7);
        if(tex8 != null) GUI.DrawTexture(new Rect(470, 160,125, 100), tex8);
        if(tex9 != null) GUI.DrawTexture(new Rect(605, 160,125, 100), tex9);

        if(downloadDone1) {
            // gzip read from buffer and decompress. gzip compress a buffer.
			if (GUI.Button(new Rect(5, 5, 180, 40), "Buffer gz test")) gzTest();
        }

        if(downloadDone2) {
            // find a gzip file merged in a buffer and decompress. The gzip attached must be created with the plugin's gzip function and the overrideDateTimeWithLength = true.
            if (GUI.Button(new Rect(5, 50, 180, 40), "start merged gzip test")) mergedTest();

            //check the compress/decompress zlib buffers section in the lzip.cs section for more variations
            if(outbuffer != null) {
                if (GUI.Button(new Rect(5, 95, 180, 40), "zlib buffer tests")) zlibTest();
            }
        }


        //--------------------------------------------------------------------------------------------------------------
        //
        // ZIP tests peformed in byte buffers. All operations are supported except those that need a file system. 
        // (Refer to the testZip.cs script for more examples that do not use the file system!)
        //
        //--------------------------------------------------------------------------------------------------------------
        if (downloadDone3) {
            if (GUI.Button(new Rect(5, 140, 180, 40), "zip tests 1")) zipTest();
		}


        // Zip tests performed using native file buffers. Avoid memory peaks during decompression.
        if (GUI.Button(new Rect(5, 185, 180, 40), "zip native buffer")) StartCoroutine(zipNativeBuffer());


		GUI.TextArea(new Rect(10, 270, Screen.width - 20, Screen.height - 280), log);
				
	}




    void gzTest() {
        log = "";
        //decompress a gzip buffer.
		plog("ungzip2: "+lzip.unGzip2(wwb, outbuffer).ToString()+" bytes"+ "\n");

		if(outbuffer != null) {
            tex.LoadImage(outbuffer);
            //compress a buffer to a gzip buffer.
            plog("gzip a buffer");
            // a temp buffer where the compression will occur.
            var tempBuffer = new byte[outbuffer.Length + 18];
            // do compression
            int res = lzip.gzip(outbuffer, tempBuffer, 9);
            // create the final gzipped buffer
            var gzippedBuffer = new byte[res];
            // copy the compressed data to it.
            Buffer.BlockCopy(tempBuffer, 0, gzippedBuffer, 0, res);
            // erase the the temp buffer. (or it could be a reusable fixed buffer to avoid allocations.)
            tempBuffer = null;
            // show decompresed and compressed sizes
            plog(outbuffer.Length.ToString() + " -> " + res.ToString()); 

            gzippedBuffer = null; // erase gzipped buffer
        }
    }

    void mergedTest() {
        log = "";

        int offset = lzip.findGzStart(wwb3);

        int gzipSize = lzip.gzipCompressedSize(wwb3, offset); 

        if(gzipSize > 0) plog("Compresed size: " + gzipSize.ToString()); else plog("No compressed size stored in gzip.");

        plog("Start of gzip File: " + offset.ToString());

        plog("unGzip2Merged: "+lzip.unGzip2Merged(wwb3, offset, wwb3.Length - offset, outbuffer2).ToString()+" bytes");

        if(outbuffer2 != null) tex2.LoadImage(outbuffer2);
    }

    void zlibTest() {
        log = "";
        var nt = lzip.compressBuffer(outbuffer, 9);
        if(nt != null){
            plog("compressed zlib buffer size: " + nt.Length.ToString());

            var cb = lzip.decompressBuffer(nt);

            if(cb != null) {
                plog("decompressed zlib buffer size: " + cb.Length.ToString());
                tex3.LoadImage(cb);
            }
            cb = null; nt = null;
        }
    }

    void zipTest() {
        log = "";

        // Handle regular zip in buffer
        //
        // get info of a zip file using the getFileInfo method.
        plog("Get Info of zip in Buffer: " + lzip.getFileInfo(null, zipwww).ToString());
        if(lzip.ninfo != null && lzip.ninfo.Count > 0) {
            for(int i=0; i < lzip.ninfo.Count; i++){
                plog("Entry no: " + (i+1).ToString() + 
                    "   " + lzip.ninfo[i] + 
                    "  uncompressed: " + lzip.uinfo[i].ToString() + 
                    "  compressed: " + lzip.cinfo[i].ToString() +
                    "  offset: " + lzip.localOffset[i].ToString());
            }
        }

        // validate a zip file in a buffer and extract an entry from it to a byte buffer.
        plog("Validate zip file in Buffer: " + lzip.validateFile(null, zipwww).ToString());
        var ob = lzip.entry2Buffer(null, "dir1/dir2/dir3/Unity_1.jpg", zipwww);
        plog("entry2Buffer: " + ob.Length.ToString());
        if(ob != null) tex4.LoadImage(ob);
        plog();


        // extract a list of entries to a newly created byte[] buffer array.
        // create the entry list first
        List<string> ent = new List<string>();
        ent.Add("dir1/dir2/test2.bmp");
        ent.Add("dir1/dir2/dir3/Unity_1.jpg");
        int eprogress = 0;

        // extract the entries and return the byte[] buffer array
        var buffers = lzip.entries2Buffers(null, ent.ToArray(), ref eprogress, zipwww);

        if(buffers != null) {
            plog("Entries to buffers: success");
            for (int i = 0; i < ent.Count; i++) { plog("index: " + i.ToString() + " - " + buffers[i].Length.ToString()); }
        }
        // nullify the buffers after use
        for (int i = 0; i < ent.Count; i++) buffers[i] = null; buffers = null; ent.Clear();
        plog();


        byte [] merged = null;

        // create an inMemory zip file.
        if(ob != null) {
            lzip.inMemory t = new lzip.inMemory();
            // compress a buffer to an in memory zip with a password. bz2 compression method is not supported.
            // The function returns the pointer to the in memory zip buffer. But you can get this also through t.pointer.
            // This function is slow when adding multiple buffers to the inMemory zip. It is better to use the low level functions below this example.
            lzip.compress_Buf2Mem(t,  9, ob, "inmem/test.jpg", null,"1234");
            // print the in memory zip size in bytes.
            plog("Create inMemory zip size: " + t.size().ToString());

            // a buffer to perform decompression operations from an in memory zip to it.
            byte[] bf = null;
            // a function that decompresses an entry from an in memory zip to a buffer that will get resized to fit the output.
            // (you can use entry2FixedBufferMem to decompress to a fixed sixed buffer or the overloaded entry2BufferMem function that returns a new buffer.)
            plog("entry2BufferMem: " + lzip.entry2BufferMem(t,"inmem/test.jpg", ref bf, "1234").ToString());

            // (!) If you don't need anymore the inMemory object use the free_inmemory function to free the occupied memory by the zip (!)
            //
            lzip.free_inmemory(t);

            // low level functions to create inMemory zip which are faster then using the above function. ------------------------------
            //
            // Create an in memory object to reference our in memory zip
            lzip.inMemory t2 = new lzip.inMemory();
            // Initiate an inMemory zip archive
            lzip.inMemoryZipStart(t2);
            // Add a buffer as a first entry in the inMemory zip
            lzip.inMemoryZipAdd(t2, 9, ob, "test.jpg");
            // Add a second buffer as the second entry.
            lzip.inMemoryZipAdd(t2, 9, ob, "directory/test2.jpg");
            // !!! -> After finishing adding buffer/files in the inMemory zip we must close it <- !!! 
            // You can reopen it later with the inMemoryZipStart function to append more entries to it.
            lzip.inMemoryZipClose(t2);
            // Write out the compressed size of the inMemory created zip
            plog("Size of Low Level inMemory zip: " + t2.size().ToString());

            byte[] bf2 = null;
            plog("entry2BufferMem low: " + lzip.entry2BufferMem(t2,"directory/test2.jpg", ref bf2).ToString());
            // make sure we got a valid inMemory zip by extracting an image out of it and loading it to a texture.
            if(bf2 != null) { tex7.LoadImage(bf2); bf2 = null; }

            // free the t2 inMemory object if you don't need it anymore.
            lzip.free_inmemory(t2);
            // end low memory functions -------------------------------------------------------------------------------------------------

            if(bf != null) {
                tex5.LoadImage(bf);
                //create a mereged zip file for the next test
                merged  = new byte[bf.Length + zipwww.Length];
                Array.Copy(bf, 0, merged, 0, bf.Length);
                Array.Copy(zipwww, 0, merged, bf.Length, zipwww.Length);
            }
        }

        // Merged zip file in buffer examples.
        //
        // -----------------------------------
        if (merged != null) {
            plog("");
            // get info of a zip file using the merged method.
            plog("Get Info of merged zip in Buffer: " + lzip.getZipInfoMerged( merged ).ToString());
            if(lzip.zinfo != null && lzip.zinfo.Count > 0) {
                for(int i=0; i < lzip.zinfo.Count; i++){
                    plog("Entry no: " + (i+1).ToString() + 
                        "   " + lzip.zinfo[i].filename + 
                        "  uncompressed: " + lzip.zinfo[i].UncompressedSize.ToString() + 
                        "  compressed: " + lzip.zinfo[i].CompressedSize.ToString() +
                        "  offset: " + lzip.zinfo[i].RelativeOffsetOfLocalFileHeader.ToString());
                }
            }

            // get the position and size of the merged zip in a buffer
            int position = 0, size = 0;
            lzip.getZipInfoMerged(merged, ref position, ref size, false);
            plog("merged zip: position = " + position.ToString() + " , size = "+ size.ToString());

            // get the pure zip file from where it is merged.
            var pureZip = lzip.getMergedZip(merged);
            if(pureZip != null) plog("got pure zip buffer with size: " + pureZip.Length.ToString());
            pureZip = null;

            // extract an entry from a merged zip
            // see lzip.cs for more overloaded functions to extract to referenced or fixed sized buffers
            var extractedData = lzip.entry2BufferMerged(merged, "dir1/dir2/dir3/Unity_1.jpg");
            if(extractedData != null) {
                tex6.LoadImage(extractedData);
                plog("entry2BufferMerged: " + extractedData.Length.ToString());
            }
        }
    }


    IEnumerator zipNativeBuffer() {

        // A native memory pointer
        IntPtr nativePointer = IntPtr.Zero;

        // Declare an int to get the downloaded archive size from the coroutine. When we pass directly a pointer to the plugin we need the size or the buffer it occupies.
        int zipSize = 0;

        log = "";

        plog("Downloading 1st file");
        plog();

        downloadDone4 = false;

        // Here we are calling the coroutine for a pointer. We also get the downloaded file size.
        // Replace the url with one that will comply with CORS headers: https://dev.to/alexandlazaris/why-isnt-my-unity-web-request-running-in-webgl-build-1lfm
         StartCoroutine(lzip.downloadZipFileNative("http://telias.free.fr/temp/testZip.zip", r => downloadDone4 = r, null, pointerResult => nativePointer = pointerResult, size => zipSize = size));

        while (!downloadDone4) yield return true;

        plog("File size: " + zipSize.ToString()); // show size of the zip

        // validate a zip file in a buffer.
        plog("Validate zip file in Buffer: " + lzip.validateFile(zipSize.ToString(), nativePointer).ToString());

        // extract an entry from it to a byte buffer (could be in a fixed or referenced buffer also)
        var ob = lzip.entry2Buffer(zipSize.ToString(), "dir1/dir2/dir3/Unity_1.jpg", nativePointer);

        // Delete the buffer from native memory.
        lzip.releaseBuffer(nativePointer);

        plog("entry2Buffer: " + ob.Length.ToString());
        if(ob != null) tex8.LoadImage(ob);

        plog();  plog();



        // Now download a zip file to a native pointer of an lzip.inMemory class
        downloadDone4 = false;

        plog("Downloading 2nd file");
        plog();

        // An inMemory lzip struct class.
        lzip.inMemory inMemZip = null;

        // Here we are calling the coroutine for an inMemory class.
        // Replace the url with one that will comply with CORS headers: https://dev.to/alexandlazaris/why-isnt-my-unity-web-request-running-in-webgl-build-1lfm
        StartCoroutine(lzip.downloadZipFileNative("http://telias.free.fr/temp/testZip.zip", r => downloadDone4 = r, result => inMemZip = result));

        while (!downloadDone4) yield return true;

        plog("File size: " + inMemZip.size().ToString());

        // validate a zip file in a buffer.
        plog("Validate zip file in Buffer: " + lzip.validateFile(inMemZip.size().ToString(), inMemZip.memoryPointer()).ToString());

        // extract an entry from it to a byte buffer (could be in a fixed or referenced buffer also)
        var ob2 = lzip.entry2Buffer(inMemZip.size().ToString(), "dir1/dir2/dir3/Unity_1.jpg", inMemZip.memoryPointer());

        // free the struct and the native memory it occupies!!!
        lzip.free_inmemory(inMemZip);

        plog("entry2Buffer: " + ob2.Length.ToString());
        if(ob2 != null) tex9.LoadImage(ob);
    }

	// ============================================================================================================================================================= 

	IEnumerator getFromSite() {
		plog("getting buffer from site ...");
		yield return true;

         using (UnityWebRequest www = UnityWebRequest.Get("https://dl.dropboxusercontent.com/s/zk118shcawkiwas/testLZ4b.png.gz")) {
            #if UNITY_5 || UNITY_4
                yield return www.Send();
            #else
                yield return www.SendWebRequest();
            #endif

            if (www.error != null)  {
                Debug.Log(www.error);
            } else {
                wwb = new byte[www.downloadHandler.data.Length]; Array.Copy(www.downloadHandler.data, 0, wwb, 0, www.downloadHandler.data.Length);
                outbuffer = new byte[ lzip.gzipUncompressedSize(wwb) ];
                plog("Got buffer");
		        downloadDone1 = true;
            }
        }


         using (UnityWebRequest www = UnityWebRequest.Get("https://dl.dropboxusercontent.com/s/874ijig3hzq1jzm/gzipMerged.jpg")) {
            #if UNITY_5 || UNITY_4
                yield return www.Send();
            #else
                yield return www.SendWebRequest();
            #endif

            if (www.error != null)  {
                Debug.Log(www.error);
            } else {
                wwb3 = new byte[www.downloadHandler.data.Length]; Array.Copy(www.downloadHandler.data, 0, wwb3, 0, www.downloadHandler.data.Length);
                outbuffer2 = new byte[ lzip.gzipUncompressedSize(wwb3) ];
                plog("Got buffer2");
		        downloadDone2 = true;
            }
        }


         using (UnityWebRequest www = UnityWebRequest.Get("https://dl.dropboxusercontent.com/s/whbz2hsuyescgej/test2Zip.zip")) {
            #if UNITY_5 || UNITY_4
                yield return www.Send();
            #else
                yield return www.SendWebRequest();
            #endif

            if (www.error != null)  {
                Debug.Log(www.error);
            } else {
                zipwww = new byte[www.downloadHandler.data.Length]; Array.Copy(www.downloadHandler.data, 0, zipwww, 0, www.downloadHandler.data.Length);
                plog("Got zip file");
                downloadDone3 = true;
            }
        }

        yield return true;

        // Run all the tests on tvos (usefull for emulator)
        #if UNITY_TVOS && !UNITY_EDITOR
            gzTest();
            mergedTest();
            zlibTest();
            zipTest();
            StartCoroutine(zipNativeBuffer());
        #endif

    }

#else
    void Start(){
        Debug.Log("Only for WebGL ot tvOS");
    }
#endif

}
