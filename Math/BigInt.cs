
using UnityEngine;

public class BigInt : MonoBehaviour
{
    public string num_A;
    public string num_B;

    public string result_Add;
    
    public int int_Mult = 2;

    public string result_Mult;

    private void OnValidate()
    {
        num_A = RestrictStringToDigits( num_A );
        num_B = RestrictStringToDigits( num_B );
        result_Add = Add(  num_A, num_B );
        result_Mult = Mult( num_A , int_Mult );
    }

    public static string Mult( string int_A, int int_B )
    {
        var carry = 0;
        var result = "";
        
        for( var i = 0; i <= int_A.Length / 3; i ++ )
        {
            var val_A = int.Parse( Split3s( int_A, i ) );

            var sum = ( val_A * int_B ) + carry;

            SumCarry( ref sum , ref carry );

            result = PrepandZeros( sum ) + result;
        }

        TrimZeros( ref result );

        return result;
    }


    public static string Add( string int_A , string int_B )
    {
        var carry = 0;
        var result = "";
        var max_len = Mathf.Max( int_A.Length, int_B.Length );

        for( var i = 0; i <= max_len / 3; i ++ )
        {
            var val_A = int.Parse( Split3s( int_A, i ) );
            var val_B = int.Parse( Split3s( int_B, i ) );

            var sum = val_A + val_B + carry;

            SumCarry( ref sum , ref carry );

            result = PrepandZeros( sum ) + result;
        }

        TrimZeros( ref result );

        return result;
    }

    static void SumCarry( ref int sum , ref int carry )
    {
        carry = 0;

        if ( sum > 999 )
        {
            carry = sum / 1000 ;
                
            sum = sum - carry * 1000;
        }
    }

    static string PrepandZeros( int value )
    {
        var result_zeros = value.ToString();

        if ( value < 100 && value > 9 ) result_zeros = "0" + value.ToString();

        else if ( value < 10 ) result_zeros = "00" + value.ToString();

        else if ( value == 0 ) result_zeros = "000";

        return result_zeros;
    }

    static void TrimZeros( ref string value )
    {
        while( value.Length > 1 && value[ 0 ] == '0' ) 
            
            value = value.Substring( 1 );
    }

    static string Split3s( string input , int index )
    {
        var start = input.Length - 3 * ( index + 1 );

        if( start < -2 ) return "0";

        if( start < 0 ) return input.Substring( 0 , 3 + start );

        return input.Substring( start, 3 );
    }

    const string DIGITS = "0123456789";
    
    static string RestrictStringToDigits( string input )
    {
        if( string.IsNullOrEmpty( input ) ) return "0";

        var output = "";
        
        for( var i = 0; i < input.Length; i++ ) 

            if( DIGITS.IndexOf( input[ i ] ) > -1 )
                
                output += input[ i ];
        
        return output;
    }
}
