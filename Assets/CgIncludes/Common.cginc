
 fixed OrenNayarDiffuse( fixed3 light, fixed3 view, fixed3 norm, fixed roughness )
{
    half VdotN = dot( view , norm );

    //the original function is LdotN = saturate( dot ( light , norm ))
    half LdotN = saturate( 4 * dot( light, norm * float3( 1 , 0.3 , 1 ) )); 
    half cos_theta_i = LdotN;
    half theta_r = acos( VdotN );
    half theta_i = acos( cos_theta_i );
    half cos_phi_diff = dot( normalize( view - norm * VdotN ),normalize( light - norm * LdotN ) );
    half alpha = max( theta_i, theta_r ) ;
    half beta = min( theta_i, theta_r ) ;
    half sigma2 = roughness * roughness;
    half A = 1.0 - 0.5 * sigma2 / (sigma2 + 0.33);
    half B = 0.45 * sigma2 / (sigma2 + 0.09);
    
    return saturate( cos_theta_i ) * (A + (B * saturate( cos_phi_diff ) * sin(alpha) * tan(beta)));
}