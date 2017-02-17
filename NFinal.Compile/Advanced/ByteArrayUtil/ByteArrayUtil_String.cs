using System;
using System.Text;

namespace NFinal.Advanced
{
    partial class ByteArrayUtil
    {
        /// <summary>
        /// ���ַ����鰴ָ������ת��Ϊ�ַ�����
        /// </summary>
        /// <param name="bytes">�ֽ�����</param>
        /// <param name="encoding">���룬
        /// ���Ϊ<c>null</c>��ʹ��<see cref="P:Settings.Global.ByteArrayEncoding" />����</param>
        /// <returns>ͨ��ת�����ɵ��ַ���</returns>
        public static string GetString(this byte[] bytes, Encoding encoding = null)
        {
            if (bytes == null) { return null; }
            if (bytes.Length == 0) { return string.Empty; }

            return (encoding ?? Settings.Global.DefaultEncoding).GetString(bytes);
        }

        /// <summary>
        /// ���ֽ����鰴ָ������ת��Ϊ�ַ�����
        /// </summary>
        /// <param name="bytes">�ֽ�����</param>
        /// <param name="encodingName">�������ƣ����Ϊ<c>null</c>��<c>string.Empty</c>��
        /// ��ʹ��<see cref="P:Settings.Global.ByteArrayEncoding" />����</param>
        /// <returns>ͨ��ת�����ɵ��ַ���</returns>
        public static string GetString(this byte[] bytes, string encodingName)
        {
            if (bytes == null) { return null; }
            if (bytes.Length == 0) { return string.Empty; }

            var encoding = string.IsNullOrWhiteSpace(encodingName)
                ? Settings.Global.DefaultEncoding
                : Encoding.GetEncoding(encodingName);

            return encoding.GetString(bytes);
        }

        /// <summary>
        /// ���ַ�����ָ���ı��ת��Ϊ�ֽ����顣
        /// </summary>
        /// <param name="s">Դ�ַ���</param>
        /// <param name="encoding">ָ��ת���ı����ʽ��
        /// ���<c>encoding</c>Ϊnull������<see cref="P:Settings.Global.ByteArrayEncoding" />����</param>
        /// <returns><c>byte[]</c>��ʾ��ת�����</returns>
        public static byte[] ToByteArray(this string s, Encoding encoding = null)
        {
            if (s == null) { return null; }
            if (s.Length == 0) { return EmptyByteArray; }

            return (encoding ?? Settings.Global.DefaultEncoding).GetBytes(s);
        }

        /// <summary>
        /// ���ַ�����ָ���ı��ת��Ϊ�ֽ����顣
        /// </summary>
        /// <param name="s">Դ�ַ���</param>
        /// <param name="encodingName">ָ��ת���ı����ʽ��
        /// ���<c>encoding</c>Ϊnull������<see cref="P:Settings.Global.ByteArrayEncoding" />����</param>
        /// <returns><c>byte[]</c>��ʾ��ת�����</returns>
        /// <exception cref="ArgumentException">
        /// <see cref="System.Text.Encoding.GetEncoding(string)"/></exception>
        public static byte[] ToByteArray(this string s, string encodingName)
        {
            if (s == null) { return null; }
            if (s.Length == 0) { return EmptyByteArray; }

            var encoding = string.IsNullOrWhiteSpace(encodingName)
                ? Settings.Global.DefaultEncoding
                : Encoding.GetEncoding(encodingName);
            return encoding.GetBytes(s);
        }
    }
}