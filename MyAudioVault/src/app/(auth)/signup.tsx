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
import { globalStyles } from '@/constants/globalStyles';
import { Colors } from '@/constants/colors';

export default function SignupScreen() {
    const { register } = useAuth();
    const [username, setUsername] = useState('');
    const [firstName, setFirstName] = useState('');
    const [lastName, setLastName] = useState('');
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [loading, setLoading] = useState(false);

    async function handleSignup() {
        if (password !== confirmPassword) {
            console.error('Signup error: Passwords do not match.');
            return;
        }

        setLoading(true);

        try {
            await register({
                username: username.trim(),
                firstName: firstName.trim(),
                lastName: lastName.trim(),
                email: email.trim(),
                password,
            });
        } catch (err) {
            console.error('Register error:', err);
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
                    <Text style={globalStyles.title}>Create Account</Text>
                    <Text style={globalStyles.subtitle}>Sign up to get started</Text>

                    <Text style={globalStyles.label}>Username</Text>
                    <TextInput
                        style={globalStyles.input}
                        value={username}
                        onChangeText={setUsername}
                        placeholder="johndoe"
                        placeholderTextColor={Colors.mainTheme.textSecondary}
                        autoCapitalize="none"
                    />

                    <View style={globalStyles.row}>
                        <View style={globalStyles.flex1}>
                            <Text style={globalStyles.label}>First Name</Text>
                            <TextInput
                                style={globalStyles.input}
                                value={firstName}
                                onChangeText={setFirstName}
                                placeholder="John"
                                placeholderTextColor={Colors.mainTheme.textSecondary}
                                autoCapitalize="words"
                            />
                        </View>
                        <View style={globalStyles.flex1}>
                            <Text style={globalStyles.label}>Last Name</Text>
                            <TextInput
                                style={globalStyles.input}
                                value={lastName}
                                onChangeText={setLastName}
                                placeholder="Doe"
                                placeholderTextColor={Colors.mainTheme.textSecondary}
                                autoCapitalize="words"
                            />
                        </View>
                    </View>

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
                    />

                    <Text style={globalStyles.label}>Password</Text>
                    <TextInput
                        style={globalStyles.input}
                        value={password}
                        onChangeText={setPassword}
                        placeholder="••••••••"
                        placeholderTextColor={Colors.mainTheme.textSecondary}
                        secureTextEntry
                    />

                    <Text style={globalStyles.label}>Confirm Password</Text>
                    <TextInput
                        style={globalStyles.input}
                        value={confirmPassword}
                        onChangeText={setConfirmPassword}
                        placeholder="••••••••"
                        placeholderTextColor={Colors.mainTheme.textSecondary}
                        secureTextEntry
                    />

                    <TouchableOpacity
                        style={[globalStyles.button, loading && globalStyles.buttonDisabled]}
                        onPress={handleSignup}
                        disabled={loading}
                    >
                        {loading ? (
                            <ActivityIndicator color={Colors.mainTheme.borderActive} />
                        ) : (
                            <Text style={globalStyles.buttonText}>Sign Up</Text>
                        )}
                    </TouchableOpacity>

                    <View style={globalStyles.footer}>
                        <Text style={globalStyles.caption}>Already have an account? </Text>
                        <Link href="/(auth)/login" asChild>
                            <TouchableOpacity>
                                <Text style={globalStyles.link}>Log In</Text>
                            </TouchableOpacity>
                        </Link>
                    </View>
                </View>
            </ScrollView>
        </KeyboardAvoidingView>
    );
}