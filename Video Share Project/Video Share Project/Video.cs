using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;
using System.Runtime.InteropServices;
using System.IO;

namespace Video_Share_Project
{

    internal class Video
    {
        public LibVLC libvlc;
        public MediaPlayer mediaPlayer;
        public bool fullscreen = false;
        public bool playing = false;
        public VideoView videoView;
        private Media currentMedia;
        
        private readonly Object locking = new object();

        public const int CHUNK_MAX_SIZE = 5 * 1024; //5KB

        public Video(VideoView view)
        {
            Core.Initialize(); //initializes libVLC package

            libvlc = new LibVLC();
            mediaPlayer = new MediaPlayer(libvlc);
            videoView = view;
            videoView.MediaPlayer = mediaPlayer;

            mediaPlayer.EndReached += EndReached;

            mediaPlayer.EncounteredError += (sender, eventArgs) => //LibVLC parameters required for adding a function
            {
                Console.WriteLine("VLC encountered an error!");
            };

            currentMedia = new Media(libvlc, "C:\\Users\\mdond\\Downloads\\zerotofive.mp4", FromType.FromPath);
            //mediaPlayer.Play(new Media(libvlc, new Uri("http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4")));
        }

        public void PlayVideo(string path = null)
        {
            if(path != null)
            {
                lock(locking)
                {
                    currentMedia = new Media(libvlc, path, FromType.FromPath);
                    mediaPlayer.Media = currentMedia;
                }
            }

            if(currentMedia != null)
            {
                Console.WriteLine("Playing segment...");
                mediaPlayer.Play();
                playing = true;
            }
            else
            {
                Console.WriteLine("No media loaded");
            }

                mediaPlayer.Play(currentMedia);
        }

        private void EndReached(object sender, EventArgs e)
        {

            Console.WriteLine("END REACHED");
            Console.WriteLine($"InvokeRequired in EndReached: {videoView.InvokeRequired}");
            if (videoView.InvokeRequired)
            {
                videoView.Invoke(new Action(() => {
                    Console.WriteLine("Got to an end of a segment");
                }));
            }

        }



        public void PlaySegment(string path)
        {
            EventHandler<System.EventArgs> handler = null; //The handler will be executed when EndReached event is being fired.
            handler = (sender, e) =>
            {
                mediaPlayer.EndReached -= handler;

                Task.Delay(500); //wait to ensure other processes finished using the segment
                if(File.Exists(path))
                {
                    Console.WriteLine($"Deleting file {path}");
                    File.Delete(path);
                }
            };
            mediaPlayer.EndReached += handler;
            //deleted the previous segment

            lock(locking)
            {
                if(currentMedia != null)
                {
                    currentMedia.Dispose();
                }
                currentMedia = new Media(libvlc, path, FromType.FromPath);
                mediaPlayer.Media = currentMedia;
            }
            Console.WriteLine("Changed current media");
            mediaPlayer.Play();
            playing = true;
        }





        //Segment is a few second part of the video.
        //Chunk will be sent in TCP, and is a part of the segment.
        //Chunk < Segment < Video
        public static List<byte[]> CreateChunksFromSegment(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"File {path} was not found");
            }

            List<byte[]> chunks = new List<byte[]>();

            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                long fileSize = stream.Length;
                int numberOfChunks = (int)Math.Ceiling((double)fileSize / CHUNK_MAX_SIZE);

                for (int i = 0; i < numberOfChunks; i++)
                {
                    long currentChunkSize = Math.Min(CHUNK_MAX_SIZE, fileSize - stream.Position);


                    byte[] chunk = new byte[currentChunkSize];

                    stream.Read(chunk, 0, (int)currentChunkSize);

                    chunks.Add(chunk);
                }
            }

            return chunks;
        }


    }
}
