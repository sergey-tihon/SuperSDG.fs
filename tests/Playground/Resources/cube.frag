#version 330 core
out vec4 FragColor;

in vec3 Normal;
in vec3 FragPos;
in vec2 TexCoords;

uniform vec3 viewPos;

struct Material {
    sampler2D diffuse;
    sampler2D specular;
    float shininess;
};
uniform Material material;

struct Light {
    vec3 position;
    vec3 direction;
    float cutOff;
    float outerCutOff;

    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
    
    float constant;
    float linear;
    float quadratic;
};
uniform Light light; 

void main()
{
    float distance = length(light.position - FragPos);
    float attenuation = 1.0 ;// (light.constant + light.linear * distance + light.quadratic * (distance * distance));
    
    // diffuse 
    vec3 norm = normalize(Normal);
    vec3 lightDir = normalize(light.position - FragPos);
    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = light.diffuse * diff * vec3(texture(material.diffuse, TexCoords));
    // ambient
    vec3 ambient = light.ambient * vec3(texture(material.diffuse, TexCoords));

    // specular
    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 reflectDir = reflect(-lightDir, norm);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), 256);
    vec3 specular = light.specular * spec * vec3(texture(material.specular, TexCoords));
    
    float theta = dot(lightDir, normalize(-light.direction));
    float epislon = light.cutOff - light.outerCutOff;
    float intensity = clamp((theta - light.outerCutOff) / epislon, 0.0, 1.0);
    FragColor = vec4((ambient + diffuse + specular) * intensity, 1.0);
} 
