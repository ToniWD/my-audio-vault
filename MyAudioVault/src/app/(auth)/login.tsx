import { useState } from 'react';
import {
    View,
    Text,
    TextInput,
    TouchableOpacity,
    ActivityIndicator,
    KeyboardAvoidingView,
    Platform,
    ScrollView,
} from 'react-native';
import { Link } from 'expo-router';
import { useAuth } from '../../hooks/useAuth';
import { Colors } from '@/constants/colors';
import { globalStyles } from '@/constants/globalStyles';

export default function LoginScreen() {
    const { login } = useAuth();
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [loading, setLoading] = useState(false);

    async function handleLogin() {
        setLoading(true);

        try {
            await login({ email: email.trim(), password });
        } catch (err) {
            console.error('Login error:', err);
        } finally {
            setLoading(false);
        }
    }

    return (
        <KeyboardAvoidingView
            style={globalStyles.screen}
            behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
        >
            <ScrollView
                contentContainerStyle={globalStyles.scrollContent}
                keyboardShouldPersistTaps="handled"
            >
                <View style={globalStyles.formGroup}>
                    <Text style={globalStyles.title}>Welcome Back</Text>
                    <Text style={globalStyles.subtitle}>Sign in to continue</Text>

                    <Text style={globalStyles.label}>Email</Text>
                    <TextInput
                        style={globalStyles.input}
                        value={email}
                        onChangeText={setEmail}
                        placeholder="you@example.com"
                        placeholderTextColor={Colors.mainTheme.textSecondary}
                        autoCapitalize="none"
                        keyboardType="email-address"
                        autoComplete="email"
                    // selectionColor={Colors.mainTheme.borderActive}
                    />

                    <Text style={globalStyles.label}>Password</Text>
                    <TextInput
                        style={globalStyles.input}
                        value={password}
                        onChangeText={setPassword}
                        placeholder="*********"
                        placeholderTextColor={Colors.mainTheme.textSecondary}
                        secureTextEntry
                        autoComplete="password"
                    />

                    <TouchableOpacity
                        style={[globalStyles.button, loading && globalStyles.buttonDisabled]}
                        onPress={handleLogin}
                        disabled={loading}
                    >
                        {loading ? (
                            <ActivityIndicator color={Colors.mainTheme.borderActive} />
                        ) : (
                            <Text style={globalStyles.buttonText}>Log In</Text>
                        )}
                    </TouchableOpacity>

                    <View style={globalStyles.footer}>
                        <Text style={globalStyles.caption}>Don't have an account? </Text>
                        <Link href="/(auth)/signup" asChild>
                            <TouchableOpacity>
                                <Text style={globalStyles.link}>Sign Up</Text>
                            </TouchableOpacity>
                        </Link>
                    </View>
                </View>
            </ScrollView>
        </KeyboardAvoidingView>
    );
}