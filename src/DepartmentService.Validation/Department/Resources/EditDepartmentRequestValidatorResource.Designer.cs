﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LT.DigitalOffice.DepartmentService.Validation.Department.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class EditDepartmentRequestValidatorResource {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal EditDepartmentRequestValidatorResource() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("LT.DigitalOffice.DepartmentService.Validation.Department.Resources.EditDepartment" +
                            "RequestValidatorResource", typeof(EditDepartmentRequestValidatorResource).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Description is too long..
        /// </summary>
        internal static string DescriptionTooLong {
            get {
                return ResourceManager.GetString("DescriptionTooLong", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Name must not be empty..
        /// </summary>
        internal static string EmptyName {
            get {
                return ResourceManager.GetString("EmptyName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Short name must not be empty..
        /// </summary>
        internal static string EmptyShortName {
            get {
                return ResourceManager.GetString("EmptyShortName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The department name already exist..
        /// </summary>
        internal static string ExistingName {
            get {
                return ResourceManager.GetString("ExistingName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The department short name already exist..
        /// </summary>
        internal static string ExistingShortName {
            get {
                return ResourceManager.GetString("ExistingShortName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Incorrect format of IsActive..
        /// </summary>
        internal static string IncorrectIsActiveFormat {
            get {
                return ResourceManager.GetString("IncorrectIsActiveFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Name is too long..
        /// </summary>
        internal static string NameTooLong {
            get {
                return ResourceManager.GetString("NameTooLong", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Category id doesn`t exist..
        /// </summary>
        internal static string NotExistingCategory {
            get {
                return ResourceManager.GetString("NotExistingCategory", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Short name is too long..
        /// </summary>
        internal static string ShortNameTooLong {
            get {
                return ResourceManager.GetString("ShortNameTooLong", resourceCulture);
            }
        }
    }
}
