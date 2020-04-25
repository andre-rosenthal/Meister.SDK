using MeisterCore;
using MeisterCore.Support;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;

namespace Meister.SDK
{
    public static class EnumUtil
    {
        public static IEnumerable<R> GetValues<T, R>()
        {
            return Enum.GetValues(typeof(T)).Cast<R>();
        }
        public static string DescriptionAttr<T>(this T source)
        {
            FieldInfo fi = source.GetType().GetField(source.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes != null && attributes.Length > 0)
                return attributes[0].Description;
            else
                return source.ToString();
        }
    }
    internal static class SdkEndPoints
    {
        public static string Lookup => "Meister.SDK.Lookup";
    }
    public class MeisterSDK : IDisposable
    {
        #region Private Session
        private MeisterSDK()
        {
        }

        private const string @default = "default";
        private bool disposedValue = false;
        private static MeisterSDK _instance;
        private MeisterSupport.MeisterExtensions MeisterExtensions { get; set; }
        private MeisterSupport.MeisterOptions MeisterOptions { get; set; }
        private MeisterSupport.RuntimeOptions MeisterRuntimeOptios { get; set; }
        private MeisterSupport.AuthenticationModes MeisterAuthenticationModes { get; set; }
        private MeisterSupport.Languages MeisterLanguageSetting { get; set; }
        #endregion
        public static MeisterSDK Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new MeisterSDK();
                return _instance;
            }
        }
        public Uri Gateway { get; set; }
        public string Client { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }
        public bool IsODataV4 { get; set; }
        public bool IsSso { get; set; }
        public bool IsOAuth2 { get; set; }
        public bool IsSaml2 { get; set; }
        public AuthenticationHeaderValue AuthenticationHeaderValue { get; set; }
        public MeisterStatus MeisterStatus { get; set; }
        public bool IsStatusOK()
        {
            return MeisterStatus.StatusCode >= HttpStatusCode.OK && MeisterStatus.StatusCode < HttpStatusCode.BadRequest;
        }
        #region Authentication
        public bool Authenticate()
        {
            MeisterExtensions = MeisterSupport.MeisterExtensions.RemoveNullsAndEmptyArrays;
            MeisterOptions = MeisterSupport.MeisterOptions.None;
            if (IsODataV4)
                MeisterOptions = MeisterSupport.MeisterOptions.UseODataV4;
            MeisterRuntimeOptios = MeisterSupport.RuntimeOptions.ExecuteSync;
            MeisterLanguageSetting = MeisterSupport.Languages.CultureBased;
            Resource<dynamic, dynamic> resource = BuildResource<dynamic, dynamic>();
            MeisterStatus = resource.Authenticate();
            if (IsStatusOK())
                return true;
            else
                return false;
        }
        #endregion
        #region Runtime Calls
        private dynamic ExecuteRequest<REQ, RES>(Resource<REQ, RES> resource, string endPoint, REQ request)
        {
            try
            {
                return resource.Execute(endPoint, request);
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                MeisterStatus = resource.MeisterStatus;
            }
        }
        private Resource<R, S> BuildResource<R, S>()
        {
            if (Client.ToLower() == @default)
                Client = string.Empty;
            else
                if (!int.TryParse(Client, out _))
                    throw new ApplicationException("Sap Client is invalid");
            return new Resource<R, S>(Gateway, AuthenticationHeaderValue, Client, MeisterExtensions, MeisterOptions, MeisterAuthenticationModes, MeisterRuntimeOptios, MeisterLanguageSetting);
        }
        #endregion
        #region IDisposable Support
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
