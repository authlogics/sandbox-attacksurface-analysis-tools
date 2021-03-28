﻿//  Copyright 2021 Google Inc. All Rights Reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.

using NtApiDotNet.Security;
using NtApiDotNet.Win32.SafeHandles;
using System;

namespace NtApiDotNet.Win32.Security.Policy
{
    /// <summary>
    /// Base class for an LSA object.
    /// </summary>
    public abstract class LsaObject : IDisposable, INtObjectSecurity
    {
        #region Private Members
        private readonly AccessMask _granted_access;
        private readonly SafeLsaHandle _handle;
        #endregion

        #region Private Protected Members
        private protected LsaObject(SafeLsaHandle handle, AccessMask granted_access, NtType nt_type, string object_name)
        {
            _handle = handle;
            _granted_access = granted_access;
            NtType = nt_type;
            ObjectName = object_name;
        }
        #endregion

        #region Internal Members
        internal SafeLsaHandle Handle => _handle;
        #endregion

        #region INtObjectSecurity Implementation

        /// <summary>
        /// Get the NT type for the object.
        /// </summary>
        public NtType NtType { get;}

        /// <summary>
        /// Get the object name for the object.
        /// </summary>
        public string ObjectName { get; }

        /// <summary>
        /// Is an access mask granted to the object.
        /// </summary>
        /// <param name="access">The access to check.</param>
        /// <returns>True if all access is granted.</returns>
        public bool IsAccessMaskGranted(AccessMask access)
        {
            // We can't tell if we really have access or not, so just assume we do.
            if (_granted_access.IsAccessGranted(GenericAccessRights.MaximumAllowed))
                return true;

            return _granted_access.IsAllAccessGranted(access);
        }

        /// <summary>
        /// Get the security descriptor specifying which parts to retrieve
        /// </summary>
        /// <param name="security_information">What parts of the security descriptor to retrieve</param>
        /// <param name="throw_on_error">True to throw on error.</param>
        /// <returns>The security descriptor</returns>
        public NtResult<SecurityDescriptor> GetSecurityDescriptor(SecurityInformation security_information, bool throw_on_error)
        {
            return _handle.QuerySecurity(security_information, LsaPolicyUtils.LsaPolicyNtType, throw_on_error);
        }

        /// <summary>
        /// Get the security descriptor specifying which parts to retrieve
        /// </summary>
        /// <param name="security_information">What parts of the security descriptor to retrieve</param>
        /// <returns>The security descriptor</returns>
        public SecurityDescriptor GetSecurityDescriptor(SecurityInformation security_information)
        {
            return GetSecurityDescriptor(security_information, true).Result;
        }

        /// <summary>
        /// Set the object's security descriptor
        /// </summary>
        /// <param name="security_descriptor">The security descriptor to set.</param>
        /// <param name="security_information">What parts of the security descriptor to set</param>
        /// <param name="throw_on_error">True to throw on error.</param>
        /// <returns>The NT status code.</returns>
        public NtStatus SetSecurityDescriptor(SecurityDescriptor security_descriptor, SecurityInformation security_information, bool throw_on_error)
        {
            return _handle.SetSecurity(security_information, security_descriptor, throw_on_error);
        }

        /// <summary>
        /// Set the object's security descriptor
        /// </summary>
        /// <param name="security_descriptor">The security descriptor to set.</param>
        /// <param name="security_information">What parts of the security descriptor to set</param>
        public void SetSecurityDescriptor(SecurityDescriptor security_descriptor, SecurityInformation security_information)
        {
            SetSecurityDescriptor(security_descriptor, security_information, true);
        }

        #endregion

        #region IDisposable implementation.
        /// <summary>
        /// Dispose the policy.
        /// </summary>
        public void Dispose()
        {
            _handle.Dispose();
        }
        #endregion
    }
}
