using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

    public static class FriendpasteClient
    {
        public static async Task<string> PostDataAsync(string postURL, string title, string body)
        {
            var request = CreateWebRequest(postURL, "POST");

            var postData = "{\"title\":\"" + title + "\"," +
                            "\"snippet\":\"" + body + "\"," +
                            "\"language\":\"text\"}";
            var data = Encoding.ASCII.GetBytes(postData);

            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                await stream.WriteAsync(data, 0, data.Length);
            }

            var response = await request.GetResponseAsync();
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            return responseString;
        }

        public static async Task PutDataAsync(string postURL, string title, string body)
        {
            var request = CreateWebRequest(postURL, "PUT");

            var putData = "{\"title\":\"" + title + "\"," +
                            "\"snippet\":\"" + body + "\"," +
                            "\"language\":\"text\"}";
            var data = Encoding.ASCII.GetBytes(putData);

            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                await stream.WriteAsync(data, 0, data.Length);
            }

            try
            {
                //await request.GetResponseAsync();            }
                request.BeginGetResponse(null, null);
                Debug.Log($"PUT sent to {postURL}");
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }
            //var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            //return responseString;
        }

        public static async Task<string> GetDataAsync(string postURL)
        {
            var request = CreateWebRequest(postURL, "GET");

            var response = await request.GetResponseAsync();
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            return responseString;
        }

        private static HttpWebRequest CreateWebRequest(string uri, string method)
        {
#pragma warning disable SYSLIB0014 // Type or member is obsolete
            var webRequest = (HttpWebRequest)WebRequest.Create(uri);
#pragma warning restore SYSLIB0014 // Type or member is obsolete

            webRequest.Method = method;
            webRequest.ContentType = "application/json";
            webRequest.Accept = "application/json";

            return webRequest;
        }
    }
