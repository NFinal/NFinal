﻿//======================================================================
//
//        Copyright : Zhengzhou Strawberry Computer Technology Co.,LTD.
//        All rights reserved
//        
//        Application:NFinal MVC framework
//        Filename : ModelHelper.cs
//        Description :把Http请求参数自动封装到自定义Model中的帮助类。
//
//        created by Lucas at  2015-5-31
//     
//        WebSite:http://www.nfinal.com
//
//======================================================================
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

namespace NFinal.Model
{
    /// <summary>
    /// 把parameters的值复制到与Model名称相同的字段或属性中的函数代理
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public delegate TModel GetModelDelegate<TModel>(NFinal.NameValueCollection parameters);
    /// <summary>
    /// 把Http请求参数自动封装到自定义Model中的帮助类。
    /// </summary>
    public class ModelHelper
    {
        /// <summary>
        /// 获取代理数据
        /// </summary>
        public struct GetModelDelegateData
        {
            /// <summary>
            /// Model类型
            /// </summary>
            public RuntimeTypeHandle modelType;
            /// <summary>
            /// 获取Model的代理函数
            /// </summary>
            public Delegate getModelDelegate;
        }
        /// <summary>
        /// 缓存得到填充某类型参数的函数代理
        /// </summary>
        public static System.Collections.Concurrent.ConcurrentDictionary<RuntimeTypeHandle, GetModelDelegateData> GetModelDictionary = new System.Collections.Concurrent.ConcurrentDictionary<RuntimeTypeHandle, GetModelDelegateData>();
        /// <summary>
        /// 获取自定义类型，并把prameters中的值复制到相应字段中
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static TModel GetModel<TModel>(NFinal.NameValueCollection parameters)
        {
            GetModelDelegateData getModelDelegateData;
            if (!GetModelDictionary.TryGetValue(typeof(TModel).TypeHandle, out getModelDelegateData))
            {
                getModelDelegateData.modelType = typeof(TModel).TypeHandle;
                getModelDelegateData.getModelDelegate = GetDelegate<TModel>(parameters);
                GetModelDictionary.TryAdd(typeof(TModel).TypeHandle, getModelDelegateData);
            }
            TModel model= ((GetModelDelegate<TModel>)getModelDelegateData.getModelDelegate)(parameters);
            return model;
        }
        /// <summary>
        /// 获取将parameters中的参数复制进自定义Model的函数代理
        /// </summary>
        public static System.Collections.Concurrent.ConcurrentDictionary<RuntimeTypeHandle, MethodInfo> StringContainerOpImplicitMethodInfoDic = new System.Collections.Concurrent.ConcurrentDictionary<RuntimeTypeHandle, MethodInfo>();
        /// <summary>
        /// 获取将parameters中的参数复制进自定义Model的函数代理
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static Delegate GetDelegate<TModel>(NFinal.NameValueCollection parameters)
        {
            System.Reflection.MethodInfo[] methodInfos = typeof(StringContainer).GetMethods();
            foreach (var methodInfo in methodInfos)
            {
                if (methodInfo.Name == "op_Implicit" && methodInfo.ReturnType != typeof(StringContainer))
                {
                    StringContainerOpImplicitMethodInfoDic.TryAdd(methodInfo.ReturnType.TypeHandle,
                        methodInfo);
                }
            }
            Delegate getModelDelegate = null;
            Type modelType = typeof(TModel);
            DynamicMethod method = new DynamicMethod("GetModelX", modelType, new Type[] {typeof(NFinal.NameValueCollection) });
            ILGenerator methodIL = method.GetILGenerator();
            var model= methodIL.DeclareLocal(modelType);
            var result = methodIL.DeclareLocal(modelType);
            var modelConstructor = modelType.GetConstructor(Type.EmptyTypes);
            //methodIL.Emit(OpCodes.Nop);
            methodIL.Emit(OpCodes.Newobj,modelConstructor);
            methodIL.Emit(OpCodes.Stloc,model);
            //var localStringVar = methodIL.DeclareLocal(typeof(string));

            Type parametersType = typeof(NameValueCollection);
            var parametersGetItem = parametersType.GetMethod("get_Item");


            System.Reflection.PropertyInfo[] modelProperties = modelType.GetProperties();
            for (int i = 0; i < modelProperties.Length; i++)
            {
                ConvertPropertyString(methodIL, model, modelProperties[i], parametersGetItem);
            }
            System.Reflection.FieldInfo[] modelFieldInfos = modelType.GetFields();
            for (int i = 0; i < modelFieldInfos.Length; i++)
            {
                ConvertFieldString(methodIL, model, modelFieldInfos[i], parametersGetItem);
            }
            //model放入栈顶
            var methodEnd = methodIL.DefineLabel();
            methodIL.Emit(OpCodes.Ldloc, model);
            methodIL.Emit(OpCodes.Stloc, result);
            methodIL.Emit(OpCodes.Br_S, methodEnd);
            methodIL.MarkLabel(methodEnd);
            methodIL.Emit(OpCodes.Ldloc, result);
            methodIL.Emit(OpCodes.Ret);
            getModelDelegate = method.CreateDelegate(typeof(GetModelDelegate<>).MakeGenericType(typeof(TModel)));
            return getModelDelegate;
        }
        /// <summary>
        /// parameters["key"]中获取值所用到的函数的反射信息
        /// </summary>
        public static readonly System.Reflection.MethodInfo NameValueCollectionGetMethodInfo = typeof(NameValueCollection).GetMethod("get_Item",new Type[] { typeof(string)});
        //public static readonly System.Reflection.MethodInfo NameValueCollectionOpImplicitMethodInfo = typeof(NameValueCollection).GetMethod("op_Implicit",new Type[] { typeof(StringContainer)});
        /// <summary>
        /// 把parameters中的值转换为Model属性中相应类型的值
        /// </summary>
        /// <param name="methodIL"></param>
        /// <param name="model"></param>
        /// <param name="propertyInfo"></param>
        /// <param name="parametersGetItem"></param>
        public static void ConvertPropertyString(ILGenerator methodIL,System.Reflection.Emit.LocalBuilder model,
            System.Reflection.PropertyInfo propertyInfo, System.Reflection.MethodInfo parametersGetItem)
        {
            Type propertyType = propertyInfo.PropertyType;
            bool isNullable = false;
            if (propertyInfo.PropertyType
#if (NET40 || NET451 || NET461)
                .IsGenericType
#endif
#if NETCORE
                .GetTypeInfo().IsGenericType
#endif
                && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                isNullable = true;
                propertyType = propertyInfo.PropertyType.GetGenericArguments()[0];
            }
            if (propertyType == typeof(System.String))
            {
                if (!propertyInfo.GetSetMethod().IsStatic)
                {
                    methodIL.Emit(OpCodes.Ldloc,model);//model
                    methodIL.Emit(OpCodes.Ldarg_0);//model,nvc
                    methodIL.Emit(OpCodes.Ldstr, propertyInfo.Name);//model,nvc,key
                    methodIL.Emit(OpCodes.Callvirt,NameValueCollectionGetMethodInfo );//model,string
                    methodIL.Emit(OpCodes.Call,StringContainerOpImplicitMethodInfoDic[typeof(String).TypeHandle]);
                    methodIL.Emit(OpCodes.Call, propertyInfo.GetSetMethod());
                }
                else
                {
                    methodIL.Emit(OpCodes.Ldarg_0);//nvc
                    methodIL.Emit(OpCodes.Ldstr, propertyInfo.Name);//nvc,key
                    methodIL.Emit(OpCodes.Callvirt, NameValueCollectionGetMethodInfo);//model,string
                    methodIL.Emit(OpCodes.Call, StringContainerOpImplicitMethodInfoDic[typeof(String).TypeHandle]);
                    methodIL.Emit(OpCodes.Call, propertyInfo.GetSetMethod());
                }
            }
            else if (propertyType == typeof(System.Int32) ||
                propertyType == typeof(System.Int16) ||
                propertyType == typeof(System.Int64) ||
                propertyType == typeof(System.UInt32) ||
                propertyType == typeof(System.UInt16) ||
                propertyType == typeof(System.UInt64) ||
                propertyType == typeof(System.Byte) ||
                propertyType == typeof(System.SByte) ||
                propertyType == typeof(System.Single) ||
                propertyType == typeof(System.Double) ||
                propertyType == typeof(System.Decimal) ||
                propertyType == typeof(System.DateTime) ||
                propertyType == typeof(System.DateTimeOffset) ||
                propertyType == typeof(System.Char) ||
                propertyType == typeof(System.Boolean) ||
                propertyType == typeof(System.Guid)
                )
            {
                if (!propertyInfo.GetSetMethod().IsStatic)
                {
                    methodIL.Emit(OpCodes.Ldloc,model);
                    methodIL.Emit(OpCodes.Ldarg_0);//nvc
                    methodIL.Emit(OpCodes.Ldstr, propertyInfo.Name);//nvc,key
                    methodIL.Emit(OpCodes.Callvirt, NameValueCollectionGetMethodInfo);//model,string
                    methodIL.Emit(OpCodes.Call, StringContainerOpImplicitMethodInfoDic[propertyInfo.PropertyType.TypeHandle]);
                    methodIL.Emit(OpCodes.Callvirt, propertyInfo.GetSetMethod());
                }
                else
                {
                    methodIL.Emit(OpCodes.Ldarg_0);//nvc
                    methodIL.Emit(OpCodes.Ldstr, propertyInfo.Name);//nvc,key
                    methodIL.Emit(OpCodes.Callvirt, NameValueCollectionGetMethodInfo);//model,string
                    methodIL.Emit(OpCodes.Call, StringContainerOpImplicitMethodInfoDic[propertyInfo.PropertyType.TypeHandle]);
                    methodIL.Emit(OpCodes.Call, propertyInfo.GetSetMethod());
                }
                if (isNullable)
                {
                    //var temp = methodIL.DeclareLocal(propertyType);
                    //var temp1 = methodIL.DeclareLocal(propertyInfo.PropertyType);
                    //var success = methodIL.DeclareLocal(typeof(Boolean));
                    //var elseCase = methodIL.DefineLabel();
                    //var endCase = methodIL.DefineLabel();
                    //methodIL.Emit(OpCodes.Ldarg_1);//nvc
                    //methodIL.Emit(OpCodes.Ldstr, propertyInfo.Name);//nvc,key
                    //methodIL.Emit(OpCodes.Callvirt, parametersGetItem);//string
                    ////string,temp
                    //methodIL.Emit(OpCodes.Ldloca_S,temp);
                    ////bool
                    //methodIL.Emit(OpCodes.Call, propertyType.GetMethod("TryParse", new Type[] { typeof(string), propertyType.MakeByRefType() }));
                    //methodIL.Emit(OpCodes.Stloc_S,success);
                    //methodIL.Emit(OpCodes.Ldloc_S,success);
                    ////
                    //methodIL.Emit(OpCodes.Brfalse_S,elseCase);
                    ////model
                    //methodIL.Emit(OpCodes.Ldarg_0);
                    ////model,temp
                    //methodIL.Emit(OpCodes.Ldloc,temp);
                    ////model.property=new Type();
                    //methodIL.Emit(OpCodes.Newobj,propertyInfo.PropertyType.GetConstructor(new Type[] { propertyType}));
                    ////model.property.setMethod(temp);
                    //methodIL.Emit(OpCodes.Callvirt,propertyInfo.GetSetMethod());
                    //methodIL.Emit(OpCodes.Br_S,endCase);

                    //methodIL.MarkLabel(elseCase);
                    
                    //methodIL.Emit(OpCodes.Ldarg_0);
                    //methodIL.Emit(OpCodes.Ldloca_S,temp1);
                    //methodIL.Emit(OpCodes.Initobj,propertyInfo.PropertyType);
                    //methodIL.Emit(OpCodes.Ldloc_S,temp1);
                    //methodIL.Emit(OpCodes.Callvirt,propertyInfo.GetSetMethod());
                    //methodIL.MarkLabel(endCase);
                }
                else
                {
                    //methodIL.Emit(OpCodes.Ldarg_1);//nvc
                    //methodIL.Emit(OpCodes.Ldstr, propertyInfo.Name);//nvc,key
                    //methodIL.Emit(OpCodes.Callvirt, parametersGetItem);//string
                    //var temp = methodIL.DeclareLocal(propertyInfo.PropertyType);


                    ////methodIL.Emit(OpCodes.Ldc_I4_0);
                    ////methodIL.Emit(OpCodes.Stloc,temp);

                    ////string,&temp
                    //methodIL.Emit(OpCodes.Ldloca_S, temp);
                    ////bool
                    //methodIL.Emit(OpCodes.Call, propertyInfo.PropertyType.GetMethod("TryParse", new Type[] { typeof(string), propertyInfo.PropertyType.MakeByRefType() }));
                    ////
                    //methodIL.Emit(OpCodes.Pop);
                    //if (!propertyInfo.GetSetMethod().IsStatic)
                    //{
                    //    //model
                    //    methodIL.Emit(OpCodes.Ldarg_0);
                    //}
                    ////model,temp
                    //methodIL.Emit(OpCodes.Ldloc, temp);
                    ////model.property=temp
                    //methodIL.Emit(OpCodes.Call, propertyInfo.GetSetMethod());
                }
            }
        }
        /// <summary>
        ///  把parameters中的值转换为Model字段中相应类型的值
        /// </summary>
        /// <param name="methodIL"></param>
        /// <param name="model"></param>
        /// <param name="fieldInfo"></param>
        /// <param name="parametersGetItem"></param>
        public static void ConvertFieldString(ILGenerator methodIL,System.Reflection.Emit.LocalBuilder model,
            System.Reflection.FieldInfo fieldInfo,System.Reflection.MethodInfo parametersGetItem)
        {
            Type fieldType = fieldInfo.FieldType;
            bool isNullable = false;
            if (fieldInfo.FieldType
#if (NET40 || NET451 || NET461)
                .IsGenericType
#endif
#if NETCORE
                .GetTypeInfo().IsGenericType
#endif
                && fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                isNullable = true;
                fieldType = fieldInfo.FieldType.GetGenericArguments()[0];
            }
            if (fieldType == typeof(System.String))
            {
                //model
                methodIL.Emit(OpCodes.Ldloc,model);
                methodIL.Emit(OpCodes.Ldarg_0);//model,nvc
                methodIL.Emit(OpCodes.Ldstr, fieldInfo.Name);//model,nvc,key
                methodIL.Emit(OpCodes.Callvirt, NameValueCollectionGetMethodInfo);//model,string
                methodIL.Emit(OpCodes.Call, StringContainerOpImplicitMethodInfoDic[typeof(String).TypeHandle]);
                //model.field=string
                methodIL.Emit(OpCodes.Stfld, fieldInfo);
            }
            else if (fieldType == typeof(System.Int32) ||
                fieldType == typeof(System.Int16) ||
                fieldType == typeof(System.Int64) ||
                fieldType == typeof(System.UInt32) ||
                fieldType == typeof(System.UInt16) ||
                fieldType == typeof(System.UInt64) ||
                fieldType == typeof(System.Byte) ||
                fieldType == typeof(System.SByte) ||
                fieldType == typeof(System.Single) ||
                fieldType == typeof(System.Double) ||
                fieldType == typeof(System.Decimal) ||
                fieldType == typeof(System.DateTime) ||
                fieldType == typeof(System.DateTimeOffset) ||
                fieldType == typeof(System.Char) ||
                fieldType == typeof(System.Boolean) ||
                fieldType == typeof(System.Guid)
                )
            {
                //model
                methodIL.Emit(OpCodes.Ldloc,model);
                methodIL.Emit(OpCodes.Ldarg_0);//model,nvc
                methodIL.Emit(OpCodes.Ldstr, fieldInfo.Name);//model,nvc,key
                methodIL.Emit(OpCodes.Callvirt, NameValueCollectionGetMethodInfo);//model,string
                methodIL.Emit(OpCodes.Call, StringContainerOpImplicitMethodInfoDic[fieldInfo.FieldType.TypeHandle]);
                //model.field=string
                methodIL.Emit(OpCodes.Stfld, fieldInfo);
                //如果是泛型
                if (isNullable)
                {

                    //var temp = methodIL.DeclareLocal(fieldType);
                    //var success = methodIL.DeclareLocal(typeof(Boolean));
                    //var elseCase = methodIL.DefineLabel();
                    //var endCase = methodIL.DefineLabel();
                    //methodIL.Emit(OpCodes.Ldarg_1);//nvc
                    //methodIL.Emit(OpCodes.Ldstr, fieldInfo.Name);//nvc,key
                    //methodIL.Emit(OpCodes.Callvirt, parametersGetItem);//call ,string
                    ////string,&temp
                    //methodIL.Emit(OpCodes.Ldloca_S,temp);
                    ////bool
                    //methodIL.Emit(OpCodes.Call, fieldType.GetMethod("TryParse", new Type[] { typeof(string), fieldType.MakeByRefType() }));
                    //methodIL.Emit(OpCodes.Stloc_S, success);
                    //methodIL.Emit(OpCodes.Ldloc_S,success);
                    ////
                    //methodIL.Emit(OpCodes.Brfalse_S, elseCase);
                    //    //model
                    //    methodIL.Emit(OpCodes.Ldarg_0);
                    //    //model,temp
                    //    methodIL.Emit(OpCodes.Ldloc,temp);
                    //    //model.filed=new Type();
                    //    methodIL.Emit(OpCodes.Newobj,fieldInfo.FieldType.GetConstructor(new Type[]{ fieldType}));
                    //    //model.field=temp;
                    //    methodIL.Emit(OpCodes.Stfld,fieldInfo);
                    //methodIL.Emit(OpCodes.Br_S,endCase);
                    //methodIL.MarkLabel(elseCase);

                    //    methodIL.Emit(OpCodes.Ldarg_0);
                    //    methodIL.Emit(OpCodes.Ldflda,fieldInfo);
                    //    methodIL.Emit(OpCodes.Initobj, fieldInfo.FieldType);

                    //methodIL.MarkLabel(endCase);
                }
                else
                {
                    //methodIL.Emit(OpCodes.Ldarg_1);//nvc
                    //methodIL.Emit(OpCodes.Ldstr, fieldInfo.Name);//key
                    //methodIL.Emit(OpCodes.Callvirt, parametersGetItem);//call ,string
                    //                                                   //get:string,model
                    //methodIL.Emit(OpCodes.Ldarg_0);
                    ////get:string,&model.field
                    //methodIL.Emit(OpCodes.Ldflda, fieldInfo);
                    ////bool
                    //methodIL.Emit(OpCodes.Call, fieldType.GetMethod("TryParse", new Type[] { typeof(string), fieldType.MakeByRefType() }));
                    ////
                    //methodIL.Emit(OpCodes.Pop);
                }
            }
            else if (fieldType == typeof(NFinal.Http.HttpMultipart.HttpFile))
            {

            }
            else if (fieldType == typeof(System.IO.Stream))
            {

            }
        }
#if EMITDEBUG
        public class ModelSample
        {
            public string a;
            public int b;
            public static float c { get; set; }
            public int? d;
            public int? e { get; set; }

        }
        public static ModelSample GetModelTest(NFinal.NameValueCollection parameters)
        {
            ModelSample sample = new Model.ModelHelper.ModelSample();
            sample.a = parameters["a"];
            sample.b = parameters["b"];
            ModelSample.c = parameters["c"];
            sample.d = parameters["d"];
            sample.e = parameters["e"];
            return sample;
        }
#endif
    }
}
