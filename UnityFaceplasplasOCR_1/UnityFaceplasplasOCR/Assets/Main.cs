using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine; 
using UnityEngine.UI;
using LitJson;
using System.Globalization;
using System.Text;
using System;

public class Main : MonoBehaviour
{
    //yanruTODO 目前的URL必须是网络图片
    // "http://wx1.sinaimg.cn/large/415f82b9ly1far7b0gamjj20kw0htt9a0.jpg";
    // http://39.105.114.112:8181/zx/static/front/img/10.png";
    //"http://up.qqjia.com/z/26/tu33256_1.jpg"; 
    //按钮上的文本
    //public Text Btn_ShibieText;
    //显示结果
    //public GameObject ShowResult;

    public Button btnStart;//开始识别网络图片的文字
    public Text DebugText;//调试log文本

    public Dropdown imgTypeDrop;//图片类型选择
    public Button btnStartCompres;//开始压缩图片

    public InputField input_src_path; //本地待压缩的图片路径
    public InputField input_http_resource_url;//网络资源文件夹路径
    public InputField input_http_image_count;//资源文件夹的图片个数


    private string ImageURL = "";// "http://39.106.119.137:9839/upload/image/testdelete/";
    //"http://39.105.114.112:8181/zx/upload/";
    private int startIndex = 1;
    private int maxCount = 3;//1;//00;//服务器图片个数
    string testLogPath = "";//写入解析结果

    private string imgTail = "jpg";//图片后缀,可能根据实际源文件进行修改
    private float maxWidth = 798f;//图片宽高的最大值，都不能超过这个数.像素最好选偶数
    private string COMPRESS_DIC = "";//@"D:\compress\"; //压缩图片的根目录
    private List<string> imgTypeList = new List<string>();

    private string outputDir = ""; //输出结果相关目录

    // Use this for initialization
    void Start()
    {
        //默认资源文件夹
        input_src_path.text = @"D:\compress";
        input_http_resource_url.text = "http://39.106.119.137:9839/upload/image/testdelete/";


        //支持的图片后缀
        imgTypeList.Add("jpeg");
        imgTypeList.Add("jpg");
        imgTypeList.Add("png");
        imgTypeDrop.ClearOptions();
        imgTypeDrop.AddOptions(imgTypeList);
        imgTypeDrop.onValueChanged.AddListener(onSelectDrop);// (index) { });//.OnSelect(delegate (name) { });


        btnStart.onClick.AddListener(delegate { TestHttpSend(); });

        btnStartCompres.onClick.AddListener(delegate { startCompressImages(); });//开始压缩图片
    }

    private void startCompressImages()
    {
        COMPRESS_DIC = input_src_path.text;
        var files = new string[0];
        try
        {
            files = Directory.GetFiles(COMPRESS_DIC);
        }
        catch (Exception e)
        {
            printLog("路径不存在!请填写有效的待处理图片路径");
            return; 
        }
         
        if (files.Length > 0)
        {
            imgTail = files[0].Substring(files[0].IndexOf(".") + 1);
            printLog("文件夹=" + COMPRESS_DIC + "下的图片后缀=" + imgTail);
        }
        for (int index = 0; index < files.Length; index++)
        {
            WXShareImage(files[index], index + 1);//重命名输出后的文件
        }

        printLog("图片全部压缩完！请将图片拷贝到资源服务器！");
    }

    private void onSelectDrop(int index)
    {
        printLog("选择的index=" + index + ",type=" + imgTypeList[index]);
        imgTail = imgTypeList[index];
    }

    //压缩图片
    public void WXShareImage(string imagePath, int fileIndex)
    {
        byte[] fileData = File.ReadAllBytes(imagePath);
        Texture2D tex = new Texture2D((int)(Screen.width), (int)(Screen.height), TextureFormat.RGB24, true);
        tex.LoadImage(fileData); 
        float max = Mathf.Max(tex.width, tex.height);
        float scale = maxWidth / max; //按最大的边进行缩放
        Texture2D temp = ScaleTexture(tex, (int)(tex.width * scale), (int)(tex.height * scale));
        byte[] pngData = temp.EncodeToJPG();
        string miniImagePath = imagePath.Replace("." + imgTail, "." + imgTail);
        miniImagePath = miniImagePath.Replace("/",@"\");
        miniImagePath = miniImagePath.Insert(miniImagePath.LastIndexOf(@"\") + 1, @"Output\");
        outputDir = "";
       for (int index=0;index<miniImagePath.Length;index++)
        {
            outputDir += miniImagePath[index];
            if (index == miniImagePath.LastIndexOf(@"\"))
            {
                break;
            }
        }
        miniImagePath = outputDir + (fileIndex) + "." + imgTail;
        printLog("保存完毕路径=" + miniImagePath + ", size=" + temp.width + "," + temp.height);
        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);//.Create(head);
        }
        File.WriteAllBytes(miniImagePath, pngData);
        Destroy(tex);
        Destroy(temp);
    }

    //该函数的接受两个参数，一个是传过来的图片路径，第二个参数是微信分享场景的id（微信文档有）。
    private Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
    {
        Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, true);
        Color[] rpixels = result.GetPixels(0);
        float incX = ((float)1 / source.width) * ((float)source.width / targetWidth);
        float incY = ((float)1 / source.height) * ((float)source.height / targetHeight);
        for (int px = 0; px < rpixels.Length; px++)
        {
            rpixels[px] = source.GetPixelBilinear(incX * ((float)px % targetWidth), incY * ((float)Mathf.Floor(px / targetWidth)));
        }
        result.SetPixels(rpixels, 0);
        result.Apply();
        return result;
    }



    private void printLog(String log)
    {
        DebugText.text += log + "\n";
        Debug.Log(log);
    }
    public void TestHttpSend()
    {
        string dateStr = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString()
            + DateTime.Now.ToShortTimeString();
        dateStr = dateStr.Replace(":", "");
        dateStr = dateStr.Replace(" ", "");
        if (input_src_path.text == "")
        {
            testLogPath = Application.dataPath + @"\Output\文字识别结果" + dateStr + ".txt";
        }
        else
        {
            string srcPath = input_src_path.text;
            srcPath = srcPath.Substring(0, srcPath.Length - srcPath.LastIndexOf(@"\") + 2);
            testLogPath = srcPath + @"\Output\文字识别结果" + dateStr + ".txt";
        }

        printLog("日志文件地址=" + testLogPath);

        startIndex = 1;// 0;
        ImageURL = input_http_resource_url.text;
        StartCoroutine(waitGetTextfromImage(ImageURL+startIndex+"."+imgTail));
    }

    private IEnumerator waitGetTextfromImage(string ImageURL)
    {
        yield return new WaitForSeconds(5f);//暂时延迟5s
        if (startIndex <= maxCount)
        {
            //识别文字
            WWWForm form = new WWWForm();
            form.AddField("api_key", "O5sY1nI8m_n3baru9LKqTtW0S5wZN7Yn");// "q8QTfr-xS5hm-i25JuWRLmWQQSHRRtzy");
            form.AddField("api_secret", "b0PqJkyvL6XaTh1BEiW216IzufM_qcdQ");// "3JAabNdllrl-Dm_-iYSG43B0ewypFlWt");
            form.AddField("image_url", ImageURL);
            printLog("即将请求imageurl=" + ImageURL);
            StartCoroutine(SendPost("https://api-cn.faceplusplus.com/imagepp/v1/recognizetext", form));
        }
        startIndex++;
    }

    //提交数据进行识别
    IEnumerator SendPost(string _url, WWWForm _wForm)
    {
        WWW postData = new WWW(_url, _wForm);
        yield return postData;
        if (postData.error != null && postData.error != "")
        {
            printLog(postData.error);
            //ShowResult.SetActive(true);
            //Btn_ShibieText.text = "识别";
            //     ShowResult.transform.Find("Text").GetComponent<Text>().text = "识别失败！";
            DebugText.text = postData.error;
            //myTimer = 2.0f;
            printLog("postData.error=" + postData.error+"@.null?"+(postData.error==null));
            string errorCode = postData.error;
            string attendCode = "";
            if (errorCode.StartsWith("400 "))
            {
                attendCode = ",1.不是图片格式 2.该图尺寸不满足要求！（大于80*80,小于800*800,且小于2MB） 3.图片URL不对";
            }
            File.WriteAllText(testLogPath, "");//清空log
            printLog("ERROER！！识别失败error=" + postData.error + attendCode);// postData.text);
        }
        else
        {
            //Btn_ShibieText.text = "识别";
            Debug.Log(postData.text); 
           // printLog(postData.text);
            DebugText.text = postData.text;
            JsonJieXi(postData.text);
        }
    }
    //解析json文本结果
    private void JsonJieXi(string str)
    {
        JsonData jd = JsonMapper.ToObject(str);
     //   Debug.Log(jd["result"].Count);

        string totalStr = str;// jd["result"].ToString();// UnicodeToString(jd["result"].ToString());
        string testStr = totalStr + "\n以下为全部value:\n";
        if (jd["result"].Count < 1)
        {
            printLog("error读到文件"+startIndex+"异常终止!!!");
        //    File.WriteAllText(testLogPath, "");
            return;
        }
        Debug.LogError("输出识别结果的路径=" + testLogPath);
        for (int i = 0; i < jd["result"].Count; i++)
        {
            Debug.Log("result=" + jd["result"].ToString() + ",count=" + jd["result"].Count);
            for (int j = 0; j < jd["result"].Count; j++)
            {
                string position = jd["result"][j]["position"].Count+"";//.ToString();
                string type = jd["result"][j]["type"].ToString();//jd["result"]["child-objects"][j]["type"].ToString();
                string value = jd["result"][j]["value"].ToString();
                Debug.Log("type["+j+"]=" + type + ",value=" + value+ ",position="+ position);
                testStr += value+"\n";// "type=" + type + ",value=" + value + "\n";
            }
            break;
        }
        File.AppendAllText(testLogPath, testStr); //叠加显示模式
    }

    public string UnicodeToString(string text)
    {
        if (string.IsNullOrEmpty(text)) return null;

        string temp = null;
        bool flag = false;

        int len = text.Length / 4;
        if (text.StartsWith("0x") || text.StartsWith("0X"))
        {
            len = text.Length / 6;//Unicode字符串中有0x
            flag = true;
        }

        StringBuilder sb = new StringBuilder(len);
        for (int i = 0; i < len; i++)
        {
            if (flag)
                temp = text.Substring(i * 6, 6).Substring(2);
            else
                temp = text.Substring(i * 4, 4);

            byte[] bytes = new byte[2];
            bytes[1] = byte.Parse(int.Parse(temp.Substring(0, 2), NumberStyles.HexNumber).ToString());
            bytes[0] = byte.Parse(int.Parse(temp.Substring(2, 2), NumberStyles.HexNumber).ToString());
            sb.Append(Encoding.Unicode.GetString(bytes));
        }
        return sb.ToString();
    }
}
