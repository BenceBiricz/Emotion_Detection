using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emotion_Detection
{
    class Helper
    {
        public static (List<FaceData>, List<FaceData>) TestTrainSplit(List<FaceData> data, float split = 1)
        {
            int numTrainSamples = (int)Math.Floor(data[0].Images.Count * split);
            int numTestSamples = data[0].Images.Count - numTrainSamples;

            List<FaceData> TestData = (from d in data
                                       select new FaceData
                                       {
                                           Images = d.Images.Take(numTestSamples).ToList(),
                                           Emotion = d.Emotion
                                       }).ToList();

            List<FaceData> TrainData = (from d in data
                                        select new FaceData
                                        {
                                            Images = d.Images.Skip(numTestSamples)
                                            .Take(numTrainSamples).ToList(),
                                            Emotion = d.Emotion
                                        }).ToList();
            return (TrainData, TestData);
        }

        public static List<FaceData> TestSample(List<FaceData> data)
        {
            int numTestSamples = data[0].Images.Count;

            List<FaceData> TestData = (from d in data
                                       select new FaceData
                                       {
                                           Images = d.Images.Take(numTestSamples).ToList(),
                                           Emotion = d.Emotion
                                       }).ToList();
            return TestData;
        }

        public static T[,] To2D<T>(T[][] source)
        {
                int FirstDim = source.Length;
                int SecondDim = source.GroupBy(row => row.Length).Single().Key;

                var result = new T[FirstDim, SecondDim];
                for (int i = 0; i < FirstDim; ++i)
                    for (int j = 0; j < SecondDim; ++j)
                        result[i, j] = source[i][j];

                return result;
        }
    }
}
